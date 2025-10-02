using Api.Application.Abstractions;
using Api.Application.Orders.Commands.Handlers;
using Api.Application.Users.Dtos;
using Api.Domain.ValueObjects;

namespace Api.Application.Users.Queries.Handlers;

public class GetUserStatementHandler : IQueryHandler<GetUserStatementQuery, UserStatementResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IBatchRepository _batchRepository;


    public GetUserStatementHandler(IUserRepository userRepository, IOrderRepository orderRepository, IBatchRepository batchRepository)
    {
        _userRepository = userRepository;
        _orderRepository = orderRepository;
        _batchRepository = batchRepository;
    }


    public async Task<UserStatementResponse> Handle(GetUserStatementQuery query, CancellationToken ct)
    {
        var userId = new UserId(query.UserId);


        var user = await _userRepository.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException("User not found.");


        var orders = await _orderRepository.GetByUserIdAsync(userId, ct);


        var batchIds = orders.Select(o => o.BatchId).Distinct().ToList();
        var batchMap = new Dictionary<BatchId, BatchNumber>();

        foreach (var batchId in batchIds)
        {
            var batch = await _batchRepository.GetByIdAsync(batchId, ct);
            if (batch is not null)
                batchMap[batchId] = batch.Number;
        }


        var orderDtos = orders.Select(order =>
        {
            var batchNumber = batchMap.TryGetValue(order.BatchId, out var number)
            ? number
            : new BatchNumber(0);


            return OrderMapper.ToDto(order, batchNumber);
        }).ToList();


        var paymentDtos = user.Payments.Select(PaymentMapper.ToDto).ToList();


        return new UserStatementResponse(
            user.Id.Value,
            user.Name.Value,
            user.TotalCharged.Amount,
            user.TotalPaid.Amount,
            user.Balance.Amount,
            orderDtos,
            paymentDtos
        );
    }
}