using QrCafe.Api.Dto.Ops;
using QrCafe.Application.Ops.Queries.GetOpsOrders;

namespace QrCafe.Api.Mappers
{
    public static class OpsOrdersMapper
    {
        public static OpsOrderListItemDto ToDto(GetOpsOrdersItem r) => new(
            r.OrderId, r.OrderType, r.TableNumber, r.CustomerName, r.DeliveryAddress, r.DeliveryReference, r.DeliveryPhone, r.Status,
            r.PaymentMethod, r.PaymentRequestedAt, r.Currency, r.DeliveryFee, r.TipAmount, r.TipSource, r.Total, r.CreatedAt,
            r.Items.Select(i => new OpsOrderItemDto(i.ItemId, i.ProductName, i.Qty, i.Notes, i.PrepStation, i.IsPrepared, i.IsDelivered, i.IsDone)).ToList()
        );
    }
}

