using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Infrastructure.Data;

namespace QrCafe.Application.Ops.Queries.GetOpsProducts
{
    public class GetOpsProductsHandler : IRequestHandler<GetOpsProductsQuery, GetOpsProductsResult>
    {
        private readonly QrCafeDbContext _db;
        public GetOpsProductsHandler(QrCafeDbContext db) => _db = db;

        public async Task<GetOpsProductsResult> Handle(GetOpsProductsQuery request, CancellationToken ct)
        {
            var q = from p in _db.Products.AsNoTracking()
                    join c in _db.Categories.AsNoTracking() on p.CategoryId equals c.Id into cc
                    from c in cc.DefaultIfEmpty()
                    where p.RestaurantId == request.RestaurantId
                        && (request.IncludeInactive || p.IsActive)
                    orderby c.Sort, p.Sort, p.Name
                    select new OpsProductItem(
                        p.Id,
                        p.Name,
                        p.Description,
                        c != null ? c.Name : null,
                        p.Price,
                        p.IsAvailable,
                        p.IsActive,
                        p.ImageUrl,
                        p.Sort
                    );

            var items = await q.ToListAsync(ct);
            return new GetOpsProductsResult(items);
        }
    }
}
