namespace Api.Application.Reporting.Dtos;

public record OrderTrackingReportItem(
    Guid OrderId,
    Guid CustomerId,
    string CustomerName,
    Guid BatchId,
    int BatchNumber,
    Guid ProductTypeId,
    string ProductTypeName,
    int Quantity,
    decimal Total,
    DateTime PlacedAt,
    DateTime? DueDate,
    string Status,
    int? DaysUntilDue
);
