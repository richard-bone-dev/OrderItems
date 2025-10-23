namespace Api.Application.Abstractions;

using Api.Application.Reporting.Dtos;

public interface IReportingService
{
    Task<IReadOnlyCollection<BatchUtilizationReportItem>> GetBatchUtilizationAsync(CancellationToken ct = default);

    Task<IReadOnlyCollection<CustomerBalanceReportItem>> GetCustomerBalancesAsync(CancellationToken ct = default);

    Task<IReadOnlyCollection<ProductRevenueReportItem>> GetRevenueByProductTypeAsync(CancellationToken ct = default);

    Task<CashFlowReport> GetCashFlowTimelineAsync(CancellationToken ct = default);

    Task<IReadOnlyCollection<OrderTrackingReportItem>> GetOperationalOrderTrackingAsync(CancellationToken ct = default);
}
