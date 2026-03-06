using QrCafe.Api.Dto.Orders;
using QrCafe.Application.Orders.Commands.CreateOrder;
using QrCafe.Application.Orders.Queries.GetOrderById;

namespace QrCafe.Api.Mappers
{
    public static class OrdersMapper
    {
        public static CreateOrderResponseDto ToDto(CreateOrderResult r) => new(
            r.OrderId, r.Status, r.Currency, r.Subtotal, r.Tax, r.Total
        );

        public static OrderPublicDto ToDto(GetOrderByIdResult r) => new(
            r.OrderId, r.OrderType, r.TableNumber, r.CustomerName, r.Status, r.PaymentStatus, r.PaymentMethod, r.Currency,
            r.Subtotal, r.Tax, r.Total, r.CreatedAt, r.OrderNumber, r.RestaurantName,
            r.Items.Select(i => new Dto.Orders.OrderItemPublicDto(i.ProductName, i.Qty, i.UnitPrice, i.LineTotal)).ToList()
        );
    }
}
