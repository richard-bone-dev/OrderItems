using Api.Application.Abstractions;
using Api.Application.Payments.Commands;
using Api.Application.Payments.Dtos;
using Api.Application.Payments.Exceptions;
using Api.Application.Payments.Queries;
using Api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Api.Tests.Controllers;

public class PaymentsControllerTests
{
    private readonly Mock<ICommandHandlerAsync<RecordPaymentCommand, PaymentDto>> _recordHandlerMock = new();
    private readonly Mock<IQueryHandlerAsync<GetCustomerPaymentsQuery, IEnumerable<PaymentDto>?>> _customerPaymentsMock = new();
    private readonly Mock<IQueryHandlerAsync<GetPaymentDetailQuery, PaymentDto?>> _detailMock = new();
    private readonly Mock<IQueryHandlerAsync<GetPaymentSnapshotQuery, PaymentDto?>> _snapshotMock = new();

    private PaymentsController CreateController()
        => new(
            _recordHandlerMock.Object,
            _customerPaymentsMock.Object,
            _detailMock.Object,
            _snapshotMock.Object);

    [Fact]
    public async Task RecordAsync_ReturnsOk_WhenPaymentIsApproved()
    {
        var command = new RecordPaymentCommand(Guid.NewGuid(), 50m, DateTime.UtcNow);
        var expected = new PaymentDto(Guid.NewGuid(), command.CustomerId, command.Amount, command.PaymentDate ?? DateTime.UtcNow);
        _recordHandlerMock
            .Setup(h => h.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController();

        var result = await controller.RecordAsync(command, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task RecordAsync_ReturnsBadRequest_WhenCommandIsNull()
    {
        var controller = CreateController();

        var result = await controller.RecordAsync(null, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RecordAsync_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        var command = new RecordPaymentCommand(Guid.NewGuid(), 50m, DateTime.UtcNow);
        var controller = CreateController();
        controller.ModelState.AddModelError("Amount", "Amount is required");

        var result = await controller.RecordAsync(command, CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RecordAsync_ReturnsBadRequest_WhenHandlerThrowsArgumentException()
    {
        var command = new RecordPaymentCommand(Guid.NewGuid(), -10m, DateTime.UtcNow);
        var errorMessage = "Invalid amount";
        _recordHandlerMock
            .Setup(h => h.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException(errorMessage));

        var controller = CreateController();

        var result = await controller.RecordAsync(command, CancellationToken.None);

        var badRequest = result.Result as BadRequestObjectResult;
        badRequest.Should().NotBeNull();
        badRequest!.Value.Should().BeEquivalentTo(new { error = errorMessage });
    }

    [Fact]
    public async Task RecordAsync_ReturnsConflict_WhenHandlerThrowsPaymentDeclinedException()
    {
        var command = new RecordPaymentCommand(Guid.NewGuid(), 25m, DateTime.UtcNow);
        var errorMessage = "Payment declined";
        _recordHandlerMock
            .Setup(h => h.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new PaymentDeclinedException(errorMessage));

        var controller = CreateController();

        var result = await controller.RecordAsync(command, CancellationToken.None);

        var conflict = result.Result as ConflictObjectResult;
        conflict.Should().NotBeNull();
        conflict!.Value.Should().BeEquivalentTo(new { error = errorMessage });
    }

    [Fact]
    public async Task RecordAsync_ReturnsNotFound_WhenHandlerThrowsKeyNotFoundException()
    {
        var command = new RecordPaymentCommand(Guid.NewGuid(), 25m, DateTime.UtcNow);
        var errorMessage = "User not found";
        _recordHandlerMock
            .Setup(h => h.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException(errorMessage));

        var controller = CreateController();

        var result = await controller.RecordAsync(command, CancellationToken.None);

        var notFound = result.Result as NotFoundObjectResult;
        notFound.Should().NotBeNull();
        notFound!.Value.Should().BeEquivalentTo(new { error = errorMessage });
    }

    [Fact]
    public async Task RecordAsync_ReturnsInternalServerError_WhenHandlerThrowsUnexpectedException()
    {
        var command = new RecordPaymentCommand(Guid.NewGuid(), 25m, DateTime.UtcNow);
        _recordHandlerMock
            .Setup(h => h.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected"));

        var controller = CreateController();

        var result = await controller.RecordAsync(command, CancellationToken.None);

        var errorResult = result.Result as ObjectResult;
        errorResult.Should().NotBeNull();
        errorResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetCustomerPaymentsAsync_ReturnsOk_WhenPaymentsExist()
    {
        var customerId = Guid.NewGuid();
        var payments = new List<PaymentDto>
        {
            new(Guid.NewGuid(), customerId, 20m, DateTime.UtcNow)
        };
        _customerPaymentsMock
            .Setup(h => h.HandleAsync(It.Is<GetCustomerPaymentsQuery>(q => q.CustomerId == customerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        var controller = CreateController();

        var result = await controller.GetCustomerPaymentsAsync(customerId, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(payments);
    }

    [Fact]
    public async Task GetCustomerPaymentsAsync_ReturnsNotFound_WhenPaymentsAreMissing()
    {
        var customerId = Guid.NewGuid();
        _customerPaymentsMock
            .Setup(h => h.HandleAsync(It.IsAny<GetCustomerPaymentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<PaymentDto>?)null);

        var controller = CreateController();

        var result = await controller.GetCustomerPaymentsAsync(customerId, CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetDetailAsync_ReturnsOk_WhenPaymentExists()
    {
        var paymentId = Guid.NewGuid();
        var expected = new PaymentDto(paymentId, Guid.NewGuid(), 15m, DateTime.UtcNow);
        _detailMock
            .Setup(h => h.HandleAsync(It.Is<GetPaymentDetailQuery>(q => q.PaymentId == paymentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController();

        var result = await controller.GetDetailAsync(paymentId, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetDetailAsync_ReturnsNotFound_WhenPaymentIsMissing()
    {
        _detailMock
            .Setup(h => h.HandleAsync(It.IsAny<GetPaymentDetailQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentDto?)null);

        var controller = CreateController();

        var result = await controller.GetDetailAsync(Guid.NewGuid(), CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetSnapshotAsync_ReturnsOk_WhenSnapshotExists()
    {
        var paymentId = Guid.NewGuid();
        var expected = new PaymentDto(paymentId, Guid.NewGuid(), 10m, DateTime.UtcNow);
        _snapshotMock
            .Setup(h => h.HandleAsync(It.Is<GetPaymentSnapshotQuery>(q => q.PaymentId == paymentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController();

        var result = await controller.GetSnapshotAsync(paymentId, CancellationToken.None);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetSnapshotAsync_ReturnsNotFound_WhenSnapshotIsMissing()
    {
        _snapshotMock
            .Setup(h => h.HandleAsync(It.IsAny<GetPaymentSnapshotQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentDto?)null);

        var controller = CreateController();

        var result = await controller.GetSnapshotAsync(Guid.NewGuid(), CancellationToken.None);

        result.Result.Should().BeOfType<NotFoundResult>();
    }
}
