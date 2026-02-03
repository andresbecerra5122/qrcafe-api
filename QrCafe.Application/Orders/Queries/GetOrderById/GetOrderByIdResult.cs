using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Orders.Queries.GetOrderById
{
    public record GetOrderByIdResult(
    Guid OrderId,
    string OrderType,
    int? TableNumber,
    string? CustomerName,
    string Status,
    string Currency,
    decimal Total,
    DateTimeOffset CreatedAt
);
}
