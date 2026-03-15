using MediatR;
using System;

namespace QrCafe.Application.Ops.Queries.GetOpsProductSalesSummary
{
    public record GetOpsProductSalesSummaryQuery(Guid RestaurantId, string Period, string Basis, string? AnchorDate)
        : IRequest<GetOpsProductSalesSummaryResult>;
}

