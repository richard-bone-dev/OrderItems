using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Persistence;
using Api.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Api.Tests.Infrastructure.Services;

public class ReportingServiceTests
{
    [Fact]
    public async Task GetCustomerBalancesAsync_TreatsNullOrderTotalsAsZero()
    {
        var seeded = await SeedNullMoneyScenarioAsync();

        await using var context = seeded.Context;
        var service = new ReportingService(context);

        var result = await service.GetCustomerBalancesAsync();

        var item = Assert.Single(result);
        Assert.Equal(seeded.Customer.Id.Value, item.CustomerId);
        Assert.Equal(0m, item.TotalCharged);
        Assert.Equal(seeded.PaymentAmount, item.TotalPaid);
        Assert.Equal(-seeded.PaymentAmount, item.Balance);
        Assert.All(item.Aging, bucket => Assert.Equal(0m, bucket.OutstandingAmount));
    }

    [Fact]
    public async Task GetBatchUtilizationAsync_TreatsNullTotalsAsZeroRevenue()
    {
        var seeded = await SeedNullMoneyScenarioAsync();

        await using var context = seeded.Context;
        var service = new ReportingService(context);

        var result = await service.GetBatchUtilizationAsync();

        var item = Assert.Single(result);
        Assert.Equal(seeded.Batch.Id.Value, item.BatchId);
        Assert.Equal(1, item.OrdersCount);
        Assert.Equal(seeded.QuantityOrdered, item.TotalQuantityOrdered);
        Assert.Equal(0m, item.TotalRevenue);
        Assert.Equal(seeded.ExpectedRemainingStock, item.RemainingStock);
    }

    [Fact]
    public async Task GetRevenueByProductTypeAsync_TreatsNullTotalsAsZeroRevenue()
    {
        var seeded = await SeedNullMoneyScenarioAsync();

        await using var context = seeded.Context;
        var service = new ReportingService(context);

        var result = await service.GetRevenueByProductTypeAsync();

        var item = Assert.Single(result);
        Assert.Equal(seeded.ProductType.Id.Value, item.ProductTypeId);
        Assert.Equal(seeded.ProductType.Name, item.ProductTypeName);
        Assert.Equal(seeded.QuantityOrdered, item.TotalQuantity);
        Assert.Equal(0m, item.TotalRevenue);
        Assert.Equal(0m, item.AveragePrice);
    }

    [Fact]
    public async Task GetCashFlowTimelineAsync_ComputesZeroCoverageWhenChargesAreNull()
    {
        var seeded = await SeedNullMoneyScenarioAsync();

        await using var context = seeded.Context;
        var service = new ReportingService(context);

        var report = await service.GetCashFlowTimelineAsync();

        Assert.Equal(0m, report.TotalCharged);
        Assert.Equal(seeded.PaymentAmount, report.TotalPaid);
        Assert.Equal(0m, report.CoveragePercentage);

        Assert.Contains(report.Timeline, point => point.Date == seeded.PlacedAt.Date && point.Charged == 0m);
        Assert.Contains(report.Timeline, point => point.Date == seeded.PaymentDate.Date && point.Paid == seeded.PaymentAmount);
        Assert.All(report.Timeline, point => Assert.Equal(0m, point.CumulativeCharged));
    }

    [Fact]
    public async Task GetOperationalOrderTrackingAsync_ReportsZeroTotalsWhenMoneyIsNull()
    {
        var seeded = await SeedNullMoneyScenarioAsync();

        await using var context = seeded.Context;
        var service = new ReportingService(context);

        var result = await service.GetOperationalOrderTrackingAsync();

        var item = Assert.Single(result);
        Assert.Equal(seeded.Customer.Id.Value, item.CustomerId);
        Assert.Equal(seeded.Batch.Id.Value, item.BatchId);
        Assert.Equal(seeded.ProductType.Id.Value, item.ProductTypeId);
        Assert.Equal(0m, item.Total);
        Assert.Equal(seeded.QuantityOrdered, item.Quantity);

        var expectedDaysUntilDue = (int?)(seeded.DueDate.Date - seeded.Today).TotalDays;
        Assert.Equal(expectedDaysUntilDue, item.DaysUntilDue);
    }

    private static async Task<SeededData> SeedNullMoneyScenarioAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        var today = DateTime.UtcNow.Date;
        var placedAt = today.AddDays(-10);
        var dueDate = today.AddDays(3);
        var paymentDate = today.AddDays(-4);

        var customer = Customer.Register(new CustomerName("Acme Corp"));
        var batch = Batch.Create(new BatchNumber(42), initialStock: 10);
        var productType = ProductType.Create("Null Widget", new Money(null));

        var order = batch.AddOrder(customer.Id, productType.Id, new Money(null), placedAt, dueDate, quantity: 2);
        customer.AddOrder(order);

        var payment = Payment.Create(customer.Id.Value, 10m, paymentDate);
        customer.AddPayment(payment);

        context.Customers.Add(customer);
        context.Batches.Add(batch);
        context.ProductTypes.Add(productType);
        context.Orders.Add(order);
        context.Payments.Add(payment);

        await context.SaveChangesAsync();

        return new SeededData(
            context,
            customer,
            batch,
            productType,
            today,
            placedAt,
            paymentDate,
            dueDate,
            quantityOrdered: 2,
            paymentAmount: 10m,
            expectedRemainingStock: 8);
    }

    private sealed record SeededData(
        ApplicationDbContext Context,
        Customer Customer,
        Batch Batch,
        ProductType ProductType,
        DateTime Today,
        DateTime PlacedAt,
        DateTime PaymentDate,
        DateTime DueDate,
        int QuantityOrdered,
        decimal PaymentAmount,
        int ExpectedRemainingStock);
}
