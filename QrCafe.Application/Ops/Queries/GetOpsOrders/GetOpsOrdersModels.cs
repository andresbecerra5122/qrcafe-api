using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Ops.Queries.GetOpsOrders
{
    public record GetOpsOrdersItem(
    Guid OrderId,
    string OrderType,
    int? TableNumber,
    string? CustomerName,
    string Status,
    string Currency,
    decimal Total,
    DateTimeOffset CreatedAt
);

    public record GetOpsOrdersResult(IReadOnlyList<GetOpsOrdersItem> Items);
}
