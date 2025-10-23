namespace Api.Application.Reporting.Dtos;

public record CashFlowPoint(
    DateTime Date,
    decimal Charged,
    decimal Paid,
    decimal CumulativeCharged,
    decimal CumulativePaid,
    decimal CoveragePercentage
);

public record CashFlowReport(
    decimal TotalCharged,
    decimal TotalPaid,
    decimal CoveragePercentage,
    IReadOnlyCollection<CashFlowPoint> Timeline
);
