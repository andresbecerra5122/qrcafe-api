using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Application.Orders.Queries.GetOrderById;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Orders.Queries.GetActiveTableOrder
{
    public class GetActiveTableOrderHandler : IRequestHandler<GetActiveTableOrderQuery, GetOrderByIdResult?>
    {
        private readonly QrCafeDbContext _db;

        public GetActiveTableOrderHandler(QrCafeDbContext db) => _db = db;

        public async Task<GetOrderByIdResult?> Handle(GetActiveTableOrderQuery request, CancellationToken ct)
        {
            var normalizedToken = request.TableToken.Trim();
            if (string.IsNullOrWhiteSpace(normalizedToken))
            {
                return null;
            }

            var orderId = await (
                from o in _db.Orders.AsNoTracking()
                join t in _db.Tables.AsNoTracking() on o.TableId equals t.Id
                where o.RestaurantId == request.RestaurantId
                      && o.OrderType == OrderType.DINE_IN
                      && t.Token == normalizedToken
                      && o.Status != OrderStatus.PAID
                      && o.Status != OrderStatus.CANCELLED
                orderby o.CreatedAt descending
                select o.Id
            ).FirstOrDefaultAsync(ct);

            if (orderId == Guid.Empty)
            {
                return null;
            }

            var orderRow = await (
                from o in _db.Orders.AsNoTracking()
                join r in _db.Restaurants.AsNoTracking() on o.RestaurantId equals r.Id
                join t in _db.Tables.AsNoTracking() on o.TableId equals t.Id into tt
                from t in tt.DefaultIfEmpty()
                join p in _db.Payments.AsNoTracking() on o.Id equals p.OrderId into pp
                from p in pp.DefaultIfEmpty()
                where o.Id == orderId
                select new
                {
                    o.Id,
                    OrderType = o.OrderType.ToString(),
                    TableNumber = t != null ? (int?)t.Number : null,
                    o.CustomerName,
                    o.DeliveryAddress,
                    o.DeliveryReference,
                    o.DeliveryPhone,
                    Status = o.Status.ToString(),
                    PaymentStatus = p != null ? p.Status.ToString() : null,
                    PaymentMethod = o.PaymentMethodLabel ?? (o.PaymentMethod != null ? o.PaymentMethod.ToString() : null),
                    o.Currency,
                    o.Subtotal,
                    o.Tax,
                    o.DeliveryFee,
                    o.TipAmount,
                    TipSource = o.TipSource != null ? o.TipSource.ToString() : null,
                    o.Total,
                    o.CreatedAt,
                    o.OrderNumber,
                    RestaurantName = r.Name
                }
            ).SingleOrDefaultAsync(ct);

            if (orderRow is null)
            {
                return null;
            }

            var items = await _db.OrderItems
                .AsNoTracking()
                .Where(i => i.OrderId == orderId)
                .OrderBy(i => i.IsPrepared)
                .ThenBy(i => i.CreatedAt)
                .Select(i => new OrderItemResult(
                    i.ProductNameSnap,
                    i.Qty,
                    i.UnitPriceSnap,
                    i.LineTotal,
                    i.PrepStation.ToString(),
                    i.IsPrepared,
                    i.IsDelivered,
                    i.IsDone
                ))
                .ToListAsync(ct);

            return new GetOrderByIdResult(
                orderRow.Id,
                orderRow.OrderType,
                orderRow.TableNumber,
                orderRow.CustomerName,
                orderRow.DeliveryAddress,
                orderRow.DeliveryReference,
                orderRow.DeliveryPhone,
                orderRow.Status,
                orderRow.PaymentStatus,
                orderRow.PaymentMethod,
                orderRow.Currency,
                orderRow.Subtotal,
                orderRow.Tax,
                orderRow.DeliveryFee,
                orderRow.TipAmount,
                orderRow.TipSource,
                orderRow.Total,
                orderRow.CreatedAt,
                orderRow.OrderNumber,
                orderRow.RestaurantName,
                items
            );
        }
    }
}
