using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Orders.Queries.GetOrderById
{
    public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, GetOrderByIdResult?>
    {
        private readonly QrCafeDbContext _db;
        public GetOrderByIdHandler(QrCafeDbContext db) => _db = db;

        public async Task<GetOrderByIdResult?> Handle(GetOrderByIdQuery request, CancellationToken ct)
        {
            var orderRow = await (
                from o in _db.Orders.AsNoTracking()
                join r in _db.Restaurants.AsNoTracking() on o.RestaurantId equals r.Id
                join t in _db.Tables.AsNoTracking() on o.TableId equals t.Id into tt
                from t in tt.DefaultIfEmpty()
                join p in _db.Payments.AsNoTracking() on o.Id equals p.OrderId into pp
                from p in pp.DefaultIfEmpty()
                where o.Id == request.OrderId
                select new
                {
                    o.Id,
                    OrderType = o.OrderType.ToString(),
                    TableNumber = t != null ? (int?)t.Number : null,
                    o.CustomerName,
                    Status = o.Status.ToString(),
                    PaymentStatus = p != null ? p.Status.ToString() : null,
                    PaymentMethod = o.PaymentMethod != null ? o.PaymentMethod.ToString() : null,
                    o.Currency,
                    o.Subtotal,
                    o.Tax,
                    o.Total,
                    o.CreatedAt,
                    o.OrderNumber,
                    RestaurantName = r.Name
                }
            ).SingleOrDefaultAsync(ct);

            if (orderRow is null) return null;

            var items = await _db.OrderItems
                .AsNoTracking()
                .Where(i => i.OrderId == request.OrderId)
                .Select(i => new OrderItemResult(
                    i.ProductNameSnap,
                    i.Qty,
                    i.UnitPriceSnap,
                    i.LineTotal
                ))
                .ToListAsync(ct);

            return new GetOrderByIdResult(
                orderRow.Id,
                orderRow.OrderType,
                orderRow.TableNumber,
                orderRow.CustomerName,
                orderRow.Status,
                orderRow.PaymentStatus,
                orderRow.PaymentMethod,
                orderRow.Currency,
                orderRow.Subtotal,
                orderRow.Tax,
                orderRow.Total,
                orderRow.CreatedAt,
                orderRow.OrderNumber,
                orderRow.RestaurantName,
                items
            );
        }
    }
}
