using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.WaiterCalls.Queries.GetWaiterCalls
{
    public class GetWaiterCallsHandler : IRequestHandler<GetWaiterCallsQuery, GetWaiterCallsResult>
    {
        private readonly QrCafeDbContext _db;
        public GetWaiterCallsHandler(QrCafeDbContext db) => _db = db;

        public async Task<GetWaiterCallsResult> Handle(GetWaiterCallsQuery request, CancellationToken ct)
        {
            var q = _db.WaiterCalls
                .AsNoTracking()
                .Where(c => c.RestaurantId == request.RestaurantId);

            if (!string.IsNullOrWhiteSpace(request.Status))
                q = q.Where(c => c.Status == request.Status);

            var items = await q
                .OrderByDescending(c => c.CreatedAt)
                .Take(100)
                .Select(c => new WaiterCallItem(
                    c.Id,
                    c.RestaurantId,
                    c.TableNumber,
                    c.Status,
                    c.CreatedAt,
                    c.AttendedAt
                ))
                .ToListAsync(ct);

            return new GetWaiterCallsResult(items);
        }
    }
}
