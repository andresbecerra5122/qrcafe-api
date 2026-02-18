using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Ops.Queries.GetOpsOrders
{
    public class GetOpsOrdersHandler : IRequestHandler<GetOpsOrdersQuery, GetOpsOrdersResult>
    {
        private readonly QrCafeDbContext _db;
        public GetOpsOrdersHandler(QrCafeDbContext db) => _db = db;

        public async Task<GetOpsOrdersResult> Handle(GetOpsOrdersQuery request, CancellationToken ct)
        {
            var statuses = new HashSet<OrderStatus>();

            if (!string.IsNullOrWhiteSpace(request.StatusCsv))
            {
                foreach (var s in request.StatusCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (Enum.TryParse<OrderStatus>(s, true, out var st))
                        statuses.Add(st);
                }
            }

            var baseQ = _db.Orders.AsNoTracking()
                .Where(o => o.RestaurantId == request.RestaurantId);

            if (statuses.Count > 0)
                baseQ = baseQ.Where(o => statuses.Contains(o.Status));

            var q = from o in baseQ
                    join t in _db.Tables.AsNoTracking() on o.TableId equals t.Id into tt
                    from t in tt.DefaultIfEmpty()
                    orderby o.CreatedAt descending
                    select new GetOpsOrdersItem(
                        o.Id,
                        o.OrderType.ToString(),
                        t != null ? t.Number : null,
                        o.CustomerName,
                        o.Status.ToString(),
                        o.PaymentMethod != null ? o.PaymentMethod.ToString() : null,
                        o.PaymentRequestedAt,
                        o.Currency,
                        o.Total,
                        o.CreatedAt
                    );

            var list = await q.Take(200).ToListAsync(ct);
            return new GetOpsOrdersResult(list);
        }
    }
}
