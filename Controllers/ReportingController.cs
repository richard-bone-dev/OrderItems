using Api.Application.Abstractions;
using Api.Application.Reporting.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/reporting")]
public class ReportingController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportingController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    /// <summary>
    /// Provides utilization, throughput, and stock information for every batch.
    /// </summary>
    [HttpGet("batch-utilization")]
    public async Task<ActionResult<IEnumerable<BatchUtilizationReportItem>>> GetBatchUtilizationAsync(CancellationToken ct)
    {
        var report = await _reportingService.GetBatchUtilizationAsync(ct);
        return Ok(report);
    }

    /// <summary>
    /// Summarizes customer balances, payments, and aging buckets.
    /// </summary>
    [HttpGet("customer-balances")]
    public async Task<ActionResult<IEnumerable<CustomerBalanceReportItem>>> GetCustomerBalancesAsync(CancellationToken ct)
    {
        var report = await _reportingService.GetCustomerBalancesAsync(ct);
        return Ok(report);
    }

    /// <summary>
    /// Aggregates revenue and quantity metrics by product type.
    /// </summary>
    [HttpGet("revenue-by-product-type")]
    public async Task<ActionResult<IEnumerable<ProductRevenueReportItem>>> GetRevenueByProductTypeAsync(CancellationToken ct)
    {
        var report = await _reportingService.GetRevenueByProductTypeAsync(ct);
        return Ok(report);
    }

    /// <summary>
    /// Returns a cash-flow timeline along with cumulative coverage metrics.
    /// </summary>
    [HttpGet("cash-flow")]
    public async Task<ActionResult<CashFlowReport>> GetCashFlowTimelineAsync(CancellationToken ct)
    {
        var report = await _reportingService.GetCashFlowTimelineAsync(ct);
        return Ok(report);
    }

    /// <summary>
    /// Lists operational order tracking information across batches and customers.
    /// </summary>
    [HttpGet("order-tracking")]
    public async Task<ActionResult<IEnumerable<OrderTrackingReportItem>>> GetOperationalOrderTrackingAsync(CancellationToken ct)
    {
        var report = await _reportingService.GetOperationalOrderTrackingAsync(ct);
        return Ok(report);
    }
}
