namespace Api.Application.Reporting.Dtos;

public record BatchUtilizationReportItem(
    Guid BatchId,
    int BatchNumber,
    DateTime CreatedAt,
    bool IsActive,
    int OrdersCount,
    int TotalQuantityOrdered,
    decimal TotalRevenue,
    int RemainingStock
);
