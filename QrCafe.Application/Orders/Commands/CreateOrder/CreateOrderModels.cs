using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Orders.Commands.CreateOrder
{
    public record CreateOrderItemInput(Guid ProductId, int Qty, string? Notes);

    public record CreateOrderInput(
        Guid RestaurantId,
        string OrderType,             // "DINE_IN" | "TAKEAWAY"
        string? TableToken,
        string? CustomerName,
        string? Notes,
        IReadOnlyList<CreateOrderItemInput> Items
    );

    public record CreateOrderResult(
        Guid OrderId,
        string Status,
        string Currency,
        decimal Subtotal,
        decimal Tax,
        decimal Total,
        long OrderNumber
    );
}
