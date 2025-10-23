using Api.Application.Abstractions;
using Api.Application.Reporting.Dtos;
using Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Services;

public class ReportingService : IReportingService
{
    private static readonly IReadOnlyList<string> _agingBucketOrder =
    [
        "Current",
        "1-30",
        "31-60",
        "61-90",
        "90+",
        "No Due Date"
    ];

    private readonly ApplicationDbContext _db;

    public ReportingService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<BatchUtilizationReportItem>> GetBatchUtilizationAsync(CancellationToken ct = default)
    {
        var batches = await _db.Batches
            .Include(b => b.Orders)
                .ThenInclude(o => o.OrderDetails)
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

        var report = batches.Select(batch =>
        {
            var orderDetails = batch.Orders.SelectMany(o => o.OrderDetails).ToList();
            var totalQuantity = orderDetails.Sum(d => d.Quantity);
            var totalRevenue = orderDetails.Sum(d => d.Total.Amount ?? 0m);

            return new BatchUtilizationReportItem(
                batch.Id.Value,
                batch.Number.Value,
                batch.CreatedAt,
                batch.IsActive,
                batch.Orders.Count,
                totalQuantity,
                totalRevenue,
                batch.Stock?.Available ?? 0
            );
        }).ToList();

        return report;
    }

    public async Task<IReadOnlyCollection<CustomerBalanceReportItem>> GetCustomerBalancesAsync(CancellationToken ct = default)
    {
        var customers = await _db.Customers
            .Include(c => c.Orders)
                .ThenInclude(o => o.OrderDetails)
            .Include(c => c.Payments)
            .AsNoTracking()
            .OrderBy(c => c.Name.Value)
            .ToListAsync(ct);

        var today = DateTime.UtcNow.Date;

        var report = new List<CustomerBalanceReportItem>();

        foreach (var customer in customers)
        {
            var orderDetails = customer.Orders
                .SelectMany(o => o.OrderDetails)
                .Select(detail => new
                {
                    Amount = detail.Total.Amount ?? 0m,
                    DueDate = detail.DueDate?.Date,
                    PlacedAt = detail.PlacedAt.Date
                })
                .OrderBy(x => x.DueDate ?? x.PlacedAt)
                .ToList();

            var totalCharged = orderDetails.Sum(d => d.Amount);
            var totalPaid = customer.Payments.Sum(p => p.PaidAmount.Amount ?? 0m);
            var balance = totalCharged - totalPaid;

            var paymentsRemaining = totalPaid;
            var bucketTotals = _agingBucketOrder.ToDictionary(name => name, _ => 0m);

            foreach (var detail in orderDetails)
            {
                var outstanding = detail.Amount;

                if (paymentsRemaining > 0)
                {
                    var applied = Math.Min(outstanding, paymentsRemaining);
                    outstanding -= applied;
                    paymentsRemaining -= applied;
                }

                if (outstanding <= 0)
                {
                    continue;
                }

                var bucketName = GetAgingBucket(detail.DueDate, today);
                bucketTotals[bucketName] += outstanding;
            }

            if (balance < 0)
            {
                // Overpayment – display as zero outstanding in buckets but keep balance negative
                foreach (var bucket in bucketTotals.Keys.ToList())
                {
                    bucketTotals[bucket] = 0m;
                }
            }

            var aging = _agingBucketOrder
                .Select(name => new AgingBucketDto(name, bucketTotals[name]))
                .ToList();

            report.Add(new CustomerBalanceReportItem(
                customer.Id.Value,
                customer.Name.Value,
                totalCharged,
                totalPaid,
                balance,
                aging
            ));
        }

        return report;
    }

    public async Task<IReadOnlyCollection<ProductRevenueReportItem>> GetRevenueByProductTypeAsync(CancellationToken ct = default)
    {
        var productTypes = await _db.ProductTypes
            .AsNoTracking()
            .ToListAsync(ct);

        var orders = await _db.Orders
            .Include(o => o.OrderDetails)
            .AsNoTracking()
            .ToListAsync(ct);

        var orderDetails = orders.SelectMany(o => o.OrderDetails);

        var grouped = orderDetails
            .GroupBy(detail => detail.ProductTypeId.Value)
            .Select(group =>
            {
                var productType = productTypes.FirstOrDefault(pt => pt.Id.Value == group.Key);
                var totalQuantity = group.Sum(d => d.Quantity);
                var totalRevenue = group.Sum(d => d.Total.Amount ?? 0m);
                var averagePrice = totalQuantity > 0 ? totalRevenue / totalQuantity : 0m;

                return new ProductRevenueReportItem(
                    group.Key,
                    productType?.Name ?? "Unknown",
                    totalQuantity,
                    totalRevenue,
                    averagePrice
                );
            })
            .OrderByDescending(item => item.TotalRevenue)
            .ToList();

        return grouped;
    }

    public async Task<CashFlowReport> GetCashFlowTimelineAsync(CancellationToken ct = default)
    {
        var orders = await _db.Orders
            .Include(o => o.OrderDetails)
            .AsNoTracking()
            .ToListAsync(ct);

        var payments = await _db.Payments
            .AsNoTracking()
            .ToListAsync(ct);

        var chargeByDate = orders
            .SelectMany(o => o.OrderDetails)
            .GroupBy(d => d.PlacedAt.Date)
            .ToDictionary(g => g.Key, g => g.Sum(d => d.Total.Amount ?? 0m));

        var paidByDate = payments
            .GroupBy(p => p.PaymentDate.Date)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.PaidAmount.Amount ?? 0m));

        var allDates = chargeByDate.Keys
            .Union(paidByDate.Keys)
            .OrderBy(date => date)
            .ToList();

        var points = new List<CashFlowPoint>();
        decimal cumulativeCharged = 0m;
        decimal cumulativePaid = 0m;

        foreach (var date in allDates)
        {
            var charged = chargeByDate.TryGetValue(date, out var c) ? c : 0m;
            var paid = paidByDate.TryGetValue(date, out var p) ? p : 0m;

            cumulativeCharged += charged;
            cumulativePaid += paid;

            var coverage = cumulativeCharged == 0m
                ? 0m
                : Math.Round((cumulativePaid / cumulativeCharged) * 100m, 2, MidpointRounding.AwayFromZero);

            points.Add(new CashFlowPoint(
                date,
                charged,
                paid,
                cumulativeCharged,
                cumulativePaid,
                coverage
            ));
        }

        var totalCharged = cumulativeCharged;
        var totalPaid = cumulativePaid;
        var totalCoverage = totalCharged == 0m
            ? 0m
            : Math.Round((totalPaid / totalCharged) * 100m, 2, MidpointRounding.AwayFromZero);

        return new CashFlowReport(
            totalCharged,
            totalPaid,
            totalCoverage,
            points
        );
    }

    public async Task<IReadOnlyCollection<OrderTrackingReportItem>> GetOperationalOrderTrackingAsync(CancellationToken ct = default)
    {
        var batches = await _db.Batches
            .AsNoTracking()
            .ToDictionaryAsync(b => b.Id.Value, b => b.Number.Value, ct);

        var customers = await _db.Customers
            .AsNoTracking()
            .ToDictionaryAsync(c => c.Id.Value, c => c.Name.Value, ct);

        var productTypes = await _db.ProductTypes
            .AsNoTracking()
            .ToDictionaryAsync(pt => pt.Id.Value, pt => pt.Name, ct);

        var orders = await _db.Orders
            .Include(o => o.OrderDetails)
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDetails.Max(d => (DateTime?)d.PlacedAt))
            .ToListAsync(ct);

        var today = DateTime.UtcNow.Date;
        var report = new List<OrderTrackingReportItem>();

        foreach (var order in orders)
        {
            var customerId = order.CustomerId.Value;
            customers.TryGetValue(customerId, out var customerName);

            batches.TryGetValue(order.BatchId.Value, out var batchNumber);

            foreach (var detail in order.OrderDetails)
            {
                productTypes.TryGetValue(detail.ProductTypeId.Value, out var productTypeName);

                var dueDate = detail.DueDate?.Date;
                string status;
                int? daysUntilDue = null;

                if (dueDate is null)
                {
                    status = "No Due Date";
                }
                else if (dueDate < today)
                {
                    status = "Overdue";
                    var daysOverdue = (today - dueDate.Value).Days;
                    daysUntilDue = -daysOverdue;
                }
                else
                {
                    var daysRemaining = (dueDate.Value - today).Days;
                    daysUntilDue = daysRemaining;
                    status = daysRemaining <= 7 ? "Due Soon" : "On Track";
                }

                report.Add(new OrderTrackingReportItem(
                    order.Id.Value,
                    customerId,
                    customerName ?? "Unknown",
                    order.BatchId.Value,
                    batchNumber,
                    detail.ProductTypeId.Value,
                    productTypeName ?? "Unknown",
                    detail.Quantity,
                    detail.Total.Amount ?? 0m,
                    detail.PlacedAt,
                    detail.DueDate,
                    status,
                    daysUntilDue
                ));
            }
        }

        return report;
    }

    private static string GetAgingBucket(DateTime? dueDate, DateTime today)
    {
        if (dueDate is null)
        {
            return "No Due Date";
        }

        var daysPastDue = (today - dueDate.Value).Days;

        if (daysPastDue <= 0)
        {
            return "Current";
        }

        return daysPastDue switch
        {
            <= 30 => "1-30",
            <= 60 => "31-60",
            <= 90 => "61-90",
            _ => "90+"
        };
    }
}
