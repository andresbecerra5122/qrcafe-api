using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QrCafe.Application.Ops.Queries.GetOpsProductSalesSummary
{
    public class GetOpsProductSalesSummaryHandler : IRequestHandler<GetOpsProductSalesSummaryQuery, GetOpsProductSalesSummaryResult>
    {
        private readonly QrCafeDbContext _db;

        public GetOpsProductSalesSummaryHandler(QrCafeDbContext db) => _db = db;

        public async Task<GetOpsProductSalesSummaryResult> Handle(GetOpsProductSalesSummaryQuery request, CancellationToken ct)
        {
            var period = NormalizePeriod(request.Period);
            var basis = NormalizeBasis(request.Basis);
            var restaurant = await _db.Restaurants.AsNoTracking()
                .Where(r => r.Id == request.RestaurantId)
                .Select(r => new { r.TimeZone })
                .SingleOrDefaultAsync(ct)
                ?? throw new KeyNotFoundException("Restaurant not found.");

            var timeZone = ResolveTimeZone(restaurant.TimeZone);
            var anchorLocalDate = ResolveAnchorLocalDate(request.AnchorDate, timeZone);
            var (rangeStartUtc, rangeEndUtc) = CalculateRangeUtc(period, timeZone, anchorLocalDate);

            var productRows = basis == "paid"
                ? await (from o in _db.Orders.AsNoTracking()
                         join i in _db.OrderItems.AsNoTracking() on o.Id equals i.OrderId
                         where o.RestaurantId == request.RestaurantId
                               && o.Status == OrderStatus.PAID
                               && (o.PaidAt ?? o.UpdatedAt) >= rangeStartUtc
                               && (o.PaidAt ?? o.UpdatedAt) < rangeEndUtc
                         select new
                         {
                             i.ProductId,
                             i.ProductNameSnap,
                             i.Qty,
                             i.LineTotal
                         }).ToListAsync(ct)
                : await (from o in _db.Orders.AsNoTracking()
                         join i in _db.OrderItems.AsNoTracking() on o.Id equals i.OrderId
                         where o.RestaurantId == request.RestaurantId
                               && o.Status != OrderStatus.CANCELLED
                               && o.CreatedAt >= rangeStartUtc
                               && o.CreatedAt < rangeEndUtc
                         select new
                         {
                             i.ProductId,
                             i.ProductNameSnap,
                             i.Qty,
                             i.LineTotal
                         }).ToListAsync(ct);

            var grouped = productRows
                .GroupBy(x => new { x.ProductId, x.ProductNameSnap })
                .Select(g => new ProductSalesSummaryItem(
                    ProductId: g.Key.ProductId,
                    ProductName: g.Key.ProductNameSnap,
                    QtySold: g.Sum(x => x.Qty),
                    Revenue: g.Sum(x => x.LineTotal)
                ))
                .OrderByDescending(x => x.Revenue)
                .ThenByDescending(x => x.QtySold)
                .ThenBy(x => x.ProductName)
                .ToList();

            var tipTotal = basis == "paid"
                ? await _db.Orders.AsNoTracking()
                    .Where(o => o.RestaurantId == request.RestaurantId
                        && o.Status == OrderStatus.PAID
                        && (o.PaidAt ?? o.UpdatedAt) >= rangeStartUtc
                        && (o.PaidAt ?? o.UpdatedAt) < rangeEndUtc)
                    .SumAsync(o => o.TipAmount, ct)
                : await _db.Orders.AsNoTracking()
                    .Where(o => o.RestaurantId == request.RestaurantId
                        && o.Status != OrderStatus.CANCELLED
                        && o.CreatedAt >= rangeStartUtc
                        && o.CreatedAt < rangeEndUtc)
                    .SumAsync(o => o.TipAmount, ct);

            return new GetOpsProductSalesSummaryResult(
                Period: period,
                Basis: basis,
                TimeZone: timeZone.Id,
                RangeStartUtc: rangeStartUtc,
                RangeEndUtc: rangeEndUtc,
                TotalItemsSold: grouped.Sum(x => x.QtySold),
                TotalRevenue: grouped.Sum(x => x.Revenue),
                TipTotal: tipTotal,
                Products: grouped
            );
        }

        private static string NormalizePeriod(string? raw)
        {
            var period = (raw ?? "day").Trim().ToLowerInvariant();
            return period switch
            {
                "day" => "day",
                "week" => "week",
                "month" => "month",
                _ => throw new ArgumentException("Invalid period. Use day, week, or month.")
            };
        }

        private static string NormalizeBasis(string? raw)
        {
            var basis = (raw ?? "paid").Trim().ToLowerInvariant();
            return basis switch
            {
                "paid" => "paid",
                "orders" => "orders",
                _ => throw new ArgumentException("Invalid basis. Use paid or orders.")
            };
        }

        private static (DateTimeOffset startUtc, DateTimeOffset endUtc) CalculateRangeUtc(
            string period,
            TimeZoneInfo timeZone,
            DateTime anchorLocalDate)
        {
            DateTime localStart;
            DateTime localEnd;

            if (period == "day")
            {
                localStart = new DateTime(anchorLocalDate.Year, anchorLocalDate.Month, anchorLocalDate.Day, 0, 0, 0, DateTimeKind.Unspecified);
                localEnd = localStart.AddDays(1);
            }
            else if (period == "week")
            {
                var daysFromMonday = ((int)anchorLocalDate.DayOfWeek + 6) % 7;
                var weekStart = anchorLocalDate.AddDays(-daysFromMonday);
                localStart = new DateTime(weekStart.Year, weekStart.Month, weekStart.Day, 0, 0, 0, DateTimeKind.Unspecified);
                localEnd = localStart.AddDays(7);
            }
            else
            {
                localStart = new DateTime(anchorLocalDate.Year, anchorLocalDate.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
                localEnd = localStart.AddMonths(1);
            }

            var startUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localStart, timeZone), TimeSpan.Zero);
            var endUtc = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localEnd, timeZone), TimeSpan.Zero);
            return (startUtc, endUtc);
        }

        private static DateTime ResolveAnchorLocalDate(string? anchorDate, TimeZoneInfo timeZone)
        {
            if (!string.IsNullOrWhiteSpace(anchorDate)
                && DateTime.TryParse(anchorDate, out var parsed))
            {
                return parsed.Date;
            }

            var nowLocal = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
            return nowLocal.Date;
        }

        private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
        {
            if (!string.IsNullOrWhiteSpace(timeZoneId))
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.Utc;
        }
    }
}

