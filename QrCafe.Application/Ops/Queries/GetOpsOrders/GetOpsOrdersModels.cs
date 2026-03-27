using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Ops.Queries.GetOpsOrders
{
    public record OpsOrderItemDetail(Guid ItemId, string ProductName, int Qty, string? Notes, string PrepStation, bool IsPrepared, bool IsDelivered, bool IsDone);

    public record GetOpsOrdersItem(
        Guid OrderId,
        string OrderType,
        int? TableNumber,
        string? CustomerName,
        string? DeliveryAddress,
        string? DeliveryReference,
        string? DeliveryPhone,
        string Status,
        string? PaymentMethod,
        DateTimeOffset? PaymentRequestedAt,
        string Currency,
        decimal DeliveryFee,
        decimal TipAmount,
        string? TipSource,
        decimal Total,
        DateTimeOffset CreatedAt,
        IReadOnlyList<OpsOrderItemDetail> Items
    );

    public record GetOpsOrdersResult(IReadOnlyList<GetOpsOrdersItem> Items);
}
