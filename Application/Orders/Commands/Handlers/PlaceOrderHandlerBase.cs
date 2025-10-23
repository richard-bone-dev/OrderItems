using System;
using System.Collections.Generic;
using Api.Application.Abstractions;
using Api.Application.Orders.Dtos;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Orders.Commands.Handlers;

public abstract class PlaceOrderHandlerBase<TCommand>
    : ICommandHandlerAsync<TCommand, OrderDto>
    where TCommand : IPlaceOrderCommand
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IBatchRepository _batchRepo;
    private readonly IUnitOfWork _unitOfWork;

    protected PlaceOrderHandlerBase(ICustomerRepository customerRepo, IBatchRepository batchRepo, IUnitOfWork unitOfWork)
    {
        _customerRepo = customerRepo;
        _batchRepo = batchRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> HandleAsync(TCommand command, CancellationToken ct = default)
    {
        var context = await LoadContextAsync(command, ct);

        var order = context.Batch.AddOrder(
            context.CustomerId,
            context.ProductTypeId,
            CalculateOrderTotal(command),
            DateTime.UtcNow,
            GetDueDate(command));

        var customerUpdated = AttachOrderToCustomer(context.Customer, order);
        customerUpdated |= await HandleCustomerUpdatesAsync(context.Customer, order, command, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return OrderMapper.ToDto(order, context.Batch.Number);
    }

    protected abstract Money CalculateOrderTotal(TCommand command);

    protected virtual DateTime? GetDueDate(TCommand command) => null;

    protected virtual Task<bool> HandleCustomerUpdatesAsync(Customer customer, Order order, TCommand command, CancellationToken ct)
        => Task.FromResult(false);

    private async Task<OrderPlacementContext> LoadContextAsync(TCommand command, CancellationToken ct)
    {
        var customerId = new CustomerId(command.CustomerId);
        var batchId = new BatchId(command.BatchId);
        var productTypeId = new ProductTypeId(command.ProductTypeId);

        var customer = await _customerRepo.GetByIdAsync(customerId, ct)
                       ?? throw new KeyNotFoundException("User not found.");
        var batch = await _batchRepo.GetByIdAsync(batchId, ct)
                    ?? throw new KeyNotFoundException("Batch not found.");

        return new OrderPlacementContext(customer, batch, customerId, productTypeId);
    }

    private static bool AttachOrderToCustomer(Customer customer, Order order)
    {
        customer.AddOrder(order);
        return true;
    }

    protected record OrderPlacementContext(Customer Customer, Batch Batch, CustomerId CustomerId, ProductTypeId ProductTypeId);
}
