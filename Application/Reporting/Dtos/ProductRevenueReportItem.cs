namespace Api.Application.Reporting.Dtos;

public record ProductRevenueReportItem(
    Guid ProductTypeId,
    string ProductTypeName,
    int TotalQuantity,
    decimal TotalRevenue,
    decimal AverageUnitPrice
);
