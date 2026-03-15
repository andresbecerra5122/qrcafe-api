using MediatR;
using System;

namespace QrCafe.Application.Ops.Queries.GetOpsSalesSummary
{
    public record GetOpsSalesSummaryQuery(Guid RestaurantId, string Period, string Basis, string? AnchorDate)
        : IRequest<GetOpsSalesSummaryResult>;
}

