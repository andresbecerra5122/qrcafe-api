using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrCafe.Api.Auth;
using QrCafe.Api.Dto.Ops;
using QrCafe.Application.Ops.Queries.GetOpsProductSalesSummary;
using QrCafe.Application.Ops.Queries.GetOpsSalesSummary;

namespace QrCafe.Api.Controllers.Public
{
    [ApiController]
    [Route("ops/reports")]
    [Authorize(Policy = AuthConstants.PolicyAdminOnly)]
    public class OpsReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OpsReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("sales-summary")]
        public async Task<ActionResult<OpsSalesSummaryDto>> GetSalesSummary(
            [FromQuery] Guid restaurantId,
            [FromQuery] string period = "day",
            [FromQuery] string basis = "paid",
            [FromQuery] string? anchorDate = null,
            CancellationToken ct = default)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            var result = await _mediator.Send(new GetOpsSalesSummaryQuery(restaurantId, period, basis, anchorDate), ct);
            return Ok(new OpsSalesSummaryDto(
                Period: result.Period,
                Basis: result.Basis,
                TimeZone: result.TimeZone,
                RangeStartUtc: result.RangeStartUtc,
                RangeEndUtc: result.RangeEndUtc,
                PaidOrdersCount: result.PaidOrdersCount,
                TotalSales: result.TotalSales,
                TipTotal: result.TipTotal,
                AverageTicket: result.AverageTicket,
                PaymentMethods: result.PaymentMethods.Select(pm => new OpsSalesPaymentMethodBreakdownDto(
                    MethodCode: pm.MethodCode,
                    MethodLabel: pm.MethodLabel,
                    Amount: pm.Amount,
                    OrdersCount: pm.OrdersCount
                )).ToList(),
                Orders: result.Orders.Select(o => new OpsSalesSummaryOrderDto(
                    OrderId: o.OrderId,
                    OrderNumber: o.OrderNumber,
                    Total: o.Total,
                    TipAmount: o.TipAmount,
                    PaymentMethodCode: o.PaymentMethodCode,
                    PaymentMethodLabel: o.PaymentMethodLabel,
                    OccurredAtUtc: o.OccurredAtUtc
                )).ToList()
            ));
        }

        [HttpGet("product-sales-summary")]
        public async Task<ActionResult<OpsProductSalesSummaryDto>> GetProductSalesSummary(
            [FromQuery] Guid restaurantId,
            [FromQuery] string period = "day",
            [FromQuery] string basis = "paid",
            [FromQuery] string? anchorDate = null,
            CancellationToken ct = default)
        {
            if (User.GetRestaurantId() != restaurantId)
            {
                return Forbid();
            }

            var result = await _mediator.Send(new GetOpsProductSalesSummaryQuery(restaurantId, period, basis, anchorDate), ct);
            return Ok(new OpsProductSalesSummaryDto(
                Period: result.Period,
                Basis: result.Basis,
                TimeZone: result.TimeZone,
                RangeStartUtc: result.RangeStartUtc,
                RangeEndUtc: result.RangeEndUtc,
                TotalItemsSold: result.TotalItemsSold,
                TotalRevenue: result.TotalRevenue,
                TipTotal: result.TipTotal,
                Products: result.Products.Select(p => new OpsProductSalesSummaryItemDto(
                    ProductId: p.ProductId,
                    ProductName: p.ProductName,
                    QtySold: p.QtySold,
                    Revenue: p.Revenue
                )).ToList()
            ));
        }
    }
}

