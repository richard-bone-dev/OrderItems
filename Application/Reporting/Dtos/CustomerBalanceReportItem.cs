namespace Api.Application.Reporting.Dtos;

public record AgingBucketDto(string Name, decimal OutstandingAmount);

public record CustomerBalanceReportItem(
    Guid CustomerId,
    string CustomerName,
    decimal TotalCharged,
    decimal TotalPaid,
    decimal Balance,
    IReadOnlyCollection<AgingBucketDto> Aging
);
