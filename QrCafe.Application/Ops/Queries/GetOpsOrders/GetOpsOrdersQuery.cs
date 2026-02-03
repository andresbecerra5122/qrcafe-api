using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Ops.Queries.GetOpsOrders
{
    public record GetOpsOrdersQuery(Guid RestaurantId, string? StatusCsv) : IRequest<GetOpsOrdersResult>;
}
