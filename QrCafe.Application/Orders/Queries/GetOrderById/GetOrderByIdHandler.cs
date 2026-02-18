using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Orders.Queries.GetOrderById
{
    public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, GetOrderByIdResult?>
    {
        private readonly QrCafeDbContext _db;
        public GetOrderByIdHandler(QrCafeDbContext db) => _db = db;

        public async Task<GetOrderByIdResult?> Handle(GetOrderByIdQuery request, CancellationToken ct)
        {
            var q = from o in _db.Orders.AsNoTracking()
                    join t in _db.Tables.AsNoTracking() on o.TableId equals t.Id into tt
                    from t in tt.DefaultIfEmpty()
                    join p in _db.Payments.AsNoTracking() on o.Id equals p.OrderId into pp
                    from p in pp.DefaultIfEmpty()
                    where o.Id == request.OrderId
                    select new GetOrderByIdResult(
                        o.Id,
                        o.OrderType.ToString(),
                        t != null ? t.Number : null,
                        o.CustomerName,
                        o.Status.ToString(),
                        p != null ? p.Status.ToString() : null,
                        o.PaymentMethod != null ? o.PaymentMethod.ToString() : null,
                        o.Currency,
                        o.Total,
                        o.CreatedAt,
                        o.OrderNumber
                    );

            return await q.SingleOrDefaultAsync(ct);
        }
    }
}
