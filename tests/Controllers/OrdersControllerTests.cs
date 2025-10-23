using System;
using System.Threading;
using System.Threading.Tasks;
using Api.Application.Abstractions;
using Api.Application.Orders.Commands;
using Api.Application.Orders.Commands.Handlers;
using Api.Application.Orders.Dtos;
using Api.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.Tests.Controllers;

public class OrdersControllerTests
{
    [Fact]
    public async Task PlaceImmediateAsync_ReturnsOk_WithOrderDto_FromHandler()
    {
        var fixture = new OrdersControllerFixture();
        var command = fixture.CreateImmediateCommand();
        var expectedDto = fixture.CreateOrderDto();
        var ct = fixture.CreateCancellationToken();

        fixture.ImmediateHandlerMock
            .Setup(h => h.HandleAsync(command, ct))
            .ReturnsAsync(expectedDto);

        var response = await fixture.Controller.PlaceImmediateAsync(command, ct);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var actualDto = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(expectedDto, actualDto);

        fixture.ImmediateHandlerMock.Verify(
            h => h.HandleAsync(
                It.Is<PlaceOrderWithImmediatePaymentCommand>(c => c == command),
                It.Is<CancellationToken>(token => token == ct)),
            Times.Once);
    }

    [Fact]
    public async Task PlaceDeferredAsync_ReturnsOk_WithOrderDto_FromHandler()
    {
        var fixture = new OrdersControllerFixture();
        var command = fixture.CreateDeferredCommand();
        var expectedDto = fixture.CreateOrderDto();
        var ct = fixture.CreateCancellationToken();

        fixture.DeferredHandlerMock
            .Setup(h => h.HandleAsync(command, ct))
            .ReturnsAsync(expectedDto);

        var response = await fixture.Controller.PlaceDeferredAsync(command, ct);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var actualDto = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(expectedDto, actualDto);

        fixture.DeferredHandlerMock.Verify(
            h => h.HandleAsync(
                It.Is<PlaceOrderWithDeferredPaymentCommand>(c => c == command),
                It.Is<CancellationToken>(token => token == ct)),
            Times.Once);
    }

    [Fact]
    public async Task PlacePartialAsync_ReturnsOk_WithOrderDto_FromHandler()
    {
        var fixture = new OrdersControllerFixture();
        var command = fixture.CreatePartialCommand();
        var expectedDto = fixture.CreateOrderDto();
        var ct = fixture.CreateCancellationToken();

        fixture.PartialHandlerMock
            .Setup(h => h.HandleAsync(command, ct))
            .ReturnsAsync(expectedDto);

        var response = await fixture.Controller.PlacePartialAsync(command, ct);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var actualDto = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(expectedDto, actualDto);

        fixture.PartialHandlerMock.Verify(
            h => h.HandleAsync(
                It.Is<PlaceOrderWithPartialPaymentCommand>(c => c == command),
                It.Is<CancellationToken>(token => token == ct)),
            Times.Once);
    }

    [Fact]
    public async Task PlaceImmediateAsync_Propagates_HandlerExceptions()
    {
        var fixture = new OrdersControllerFixture();
        var command = fixture.CreateImmediateCommand();
        var ct = fixture.CreateCancellationToken();
        var exception = new InvalidOperationException("test-error");

        fixture.ImmediateHandlerMock
            .Setup(h => h.HandleAsync(command, ct))
            .ThrowsAsync(exception);

        var actual = await Assert.ThrowsAsync<InvalidOperationException>(
            () => fixture.Controller.PlaceImmediateAsync(command, ct));

        Assert.Same(exception, actual);
    }

    [Fact]
    public async Task PlaceDeferredAsync_WhenCancellationRequested_PropagatesOperationCanceled()
    {
        var fixture = new OrdersControllerFixture();
        var command = fixture.CreateDeferredCommand();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var ct = cts.Token;

        fixture.DeferredHandlerMock
            .Setup(h => h.HandleAsync(command, ct))
            .ThrowsAsync(new OperationCanceledException(ct));

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => fixture.Controller.PlaceDeferredAsync(command, ct));

        fixture.DeferredHandlerMock.Verify(
            h => h.HandleAsync(
                It.Is<PlaceOrderWithDeferredPaymentCommand>(c => c == command),
                It.Is<CancellationToken>(token => token == ct)),
            Times.Once);
    }

    [Fact]
    public async Task PlacePartialAsync_WhenHandlerThrows_ReturnsException()
    {
        var fixture = new OrdersControllerFixture();
        var command = fixture.CreatePartialCommand();
        var ct = fixture.CreateCancellationToken();

        fixture.PartialHandlerMock
            .Setup(h => h.HandleAsync(command, ct))
            .ThrowsAsync(new Exception("handler failed"));

        await Assert.ThrowsAsync<Exception>(
            () => fixture.Controller.PlacePartialAsync(command, ct));
    }

    private sealed class OrdersControllerFixture
    {
        public OrdersControllerFixture()
        {
            ImmediateHandlerMock = CreateHandlerMock<PlaceOrderWithImmediatePaymentHandler>();
            DeferredHandlerMock = CreateHandlerMock<PlaceOrderWithDeferredPaymentHandler>();
            PartialHandlerMock = CreateHandlerMock<PlaceOrderWithPartialPaymentHandler>();

            Controller = new OrdersController(
                ImmediateHandlerMock.Object,
                DeferredHandlerMock.Object,
                PartialHandlerMock.Object);
        }

        public OrdersController Controller { get; }

        public Mock<PlaceOrderWithImmediatePaymentHandler> ImmediateHandlerMock { get; }

        public Mock<PlaceOrderWithDeferredPaymentHandler> DeferredHandlerMock { get; }

        public Mock<PlaceOrderWithPartialPaymentHandler> PartialHandlerMock { get; }

        public PlaceOrderWithImmediatePaymentCommand CreateImmediateCommand()
            => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 12.34m);

        public PlaceOrderWithDeferredPaymentCommand CreateDeferredCommand()
            => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 88.00m, DateTime.UtcNow.AddDays(5));

        public PlaceOrderWithPartialPaymentCommand CreatePartialCommand()
            => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 50.00m, 25.00m, DateTime.UtcNow.AddDays(10));

        public OrderDto CreateOrderDto()
            => new(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                42,
                Guid.NewGuid(),
                5.50m,
                3,
                16.50m,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1));

        public CancellationToken CreateCancellationToken()
            => CancellationToken.None;

        private static Mock<THandler> CreateHandlerMock<THandler>()
            where THandler : class
        {
            return new Mock<THandler>(
                MockBehavior.Strict,
                Mock.Of<ICustomerRepository>(),
                Mock.Of<IBatchRepository>(),
                Mock.Of<IUnitOfWork>());
        }
    }
}
