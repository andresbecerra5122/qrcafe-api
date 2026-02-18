using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrCafe.Application.Tables.Queries.ResolveTable
{
    public class ResolveTableHandler : IRequestHandler<ResolveTableQuery, ResolveTableResult?>
    {
        private readonly QrCafeDbContext _db;
        public ResolveTableHandler(QrCafeDbContext db) => _db = db;

        public async Task<ResolveTableResult?> Handle(ResolveTableQuery q, CancellationToken ct)
        {
            var number = q.Number?.Trim();
            if (string.IsNullOrWhiteSpace(number)) return null;

            var table = await _db.Tables
                .AsNoTracking()
                .Where(t => t.RestaurantId == q.RestaurantId && t.IsActive)
                .FirstOrDefaultAsync(t => t.Token.ToLower() == number.ToLower(), ct);

            if (table is null) return null;

            return new ResolveTableResult(table.Number, table.Token);
        }
    }
}
