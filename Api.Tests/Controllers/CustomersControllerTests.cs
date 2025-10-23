using Api.Application.Abstractions;
using Api.Application.Customers.Commands;
using Api.Application.Customers.Commands.Handlers;
using Api.Application.Customers.Dtos;
using Api.Application.Customers.Queries;
using Api.Application.Customers.Queries.Handlers;
using Api.Controllers;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Api.Tests.Controllers;

public class CustomersControllerTests
{
    [Fact]
    public async Task CreateAsync_ReturnsCreatedAtAction_WithCustomer()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var repositoryMock = new Mock<ICustomerRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var batchRepositoryMock = new Mock<IBatchRepository>();

        repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var controller = CreateController(
            repositoryMock.Object,
            unitOfWorkMock.Object,
            dbContext,
            orderRepositoryMock.Object,
            batchRepositoryMock.Object);

        var command = new CreateCustomerCommand("Alice");

        // Act
        var actionResult = await controller.CreateAsync(command, CancellationToken.None);

        // Assert
        var createdResult = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(CustomersController.GetByIdAsync));

        var dto = createdResult.Value.Should().BeOfType<CustomerDto>().Subject;
        dto.Name.Should().Be(command.Name);
        createdResult.RouteValues.Should().ContainKey("customerId");

        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var repositoryMock = new Mock<ICustomerRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var controller = CreateController(
            repositoryMock.Object,
            unitOfWorkMock.Object,
            dbContext,
            new Mock<IOrderRepository>().Object,
            new Mock<IBatchRepository>().Object);

        controller.ModelState.AddModelError("Name", "Required");
        var command = new CreateCustomerCommand("Alice");

        // Act
        var actionResult = await controller.CreateAsync(command, CancellationToken.None);

        // Assert
        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();

        repositoryMock.Verify(r => r.AddAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCustomersAsync_ReturnsOk_WithCustomers()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var repositoryMock = new Mock<ICustomerRepository>();
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var batchRepositoryMock = new Mock<IBatchRepository>();

        var customerOne = Customer.Register(new CustomerName("Alice"));
        var customerTwo = Customer.Register(new CustomerName("Bob"));

        await dbContext.Customers.AddRangeAsync(customerOne, customerTwo);
        await dbContext.SaveChangesAsync();

        var controller = CreateController(
            repositoryMock.Object,
            new Mock<IUnitOfWork>().Object,
            dbContext,
            orderRepositoryMock.Object,
            batchRepositoryMock.Object);

        // Act
        var actionResult = await controller.GetCustomersAsync(CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var customers = okResult.Value.Should().BeAssignableTo<IEnumerable<CustomerDto>>().Subject;
        customers.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerExists_ReturnsOk()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var repositoryMock = new Mock<ICustomerRepository>();
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var batchRepositoryMock = new Mock<IBatchRepository>();

        var customer = Customer.Register(new CustomerName("Alice"));
        await dbContext.Customers.AddAsync(customer);
        await dbContext.SaveChangesAsync();

        var controller = CreateController(
            repositoryMock.Object,
            new Mock<IUnitOfWork>().Object,
            dbContext,
            orderRepositoryMock.Object,
            batchRepositoryMock.Object);

        // Act
        var actionResult = await controller.GetByIdAsync(customer.Id.Value, CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = okResult.Value.Should().BeOfType<CustomerDto>().Subject;
        dto.Id.Should().Be(customer.Id.Value);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCustomerDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var controller = CreateController(
            new Mock<ICustomerRepository>().Object,
            new Mock<IUnitOfWork>().Object,
            dbContext,
            new Mock<IOrderRepository>().Object,
            new Mock<IBatchRepository>().Object);

        // Act
        var actionResult = await controller.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        actionResult.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetStatementAsync_WhenCustomerExists_ReturnsOk()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var repositoryMock = new Mock<ICustomerRepository>();
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var batchRepositoryMock = new Mock<IBatchRepository>();

        var customer = Customer.Register(new CustomerName("Alice"));
        var payment = Payment.Create(customer.Id.Value, 50m, DateTime.UtcNow);
        customer.AddPayment(payment);

        var batch = Batch.Create(new BatchNumber(7), 5);
        var detail = new OrderDetail(ProductTypeId.New(), new Money(100m), DateTime.UtcNow, quantity: 2);
        var order = Order.Create(customer.Id, batch.Id, new List<OrderDetail> { detail });
        customer.AddOrder(order);

        repositoryMock
            .Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        orderRepositoryMock
            .Setup(r => r.GetByCustomerIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { order });

        batchRepositoryMock
            .Setup(r => r.GetByIdAsync(batch.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(batch);

        var controller = CreateController(
            repositoryMock.Object,
            new Mock<IUnitOfWork>().Object,
            dbContext,
            orderRepositoryMock.Object,
            batchRepositoryMock.Object);

        // Act
        var actionResult = await controller.GetStatementAsync(customer.Id.Value, CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var statement = okResult.Value.Should().BeOfType<CustomerStatementResponse>().Subject;
        statement.CustomerId.Should().Be(customer.Id.Value);
        statement.Orders.Should().ContainSingle();
        statement.Payments.Should().ContainSingle();
    }

    [Fact]
    public async Task GetStatementAsync_WhenCustomerIsMissing_ReturnsNotFound()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var repositoryMock = new Mock<ICustomerRepository>();
        var orderRepositoryMock = new Mock<IOrderRepository>();
        var batchRepositoryMock = new Mock<IBatchRepository>();

        repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<CustomerId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var controller = CreateController(
            repositoryMock.Object,
            new Mock<IUnitOfWork>().Object,
            dbContext,
            orderRepositoryMock.Object,
            batchRepositoryMock.Object);

        // Act
        var actionResult = await controller.GetStatementAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        var notFound = actionResult.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.Value.Should().Be("User not found.");
    }

    private static CustomersController CreateController(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext dbContext,
        IOrderRepository orderRepository,
        IBatchRepository batchRepository)
    {
        return new CustomersController(
            new CreateCustomerHandler(customerRepository, unitOfWork),
            new GetCustomersHandler(dbContext),
            new GetCustomerStatementHandler(customerRepository, orderRepository, batchRepository));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
