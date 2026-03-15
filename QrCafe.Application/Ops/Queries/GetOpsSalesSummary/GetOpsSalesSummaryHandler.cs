using MediatR;
using Microsoft.EntityFrameworkCore;
using QrCafe.Domain.Entities.Enums;
using QrCafe.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QrCafe.Application.Ops.Queries.GetOpsSalesSummary
{
    public class GetOpsSalesSummaryHandler : IRequestHandler<GetOpsSalesSummaryQuery, GetOpsSalesSummaryResult>
    {
        private readonly QrCafeDbContext _db;

        public GetOpsSalesSummaryHandler(QrCafeDbContext db) => _db = db;

        public async Task<GetOpsSalesSummaryResult> Handle(GetOpsSalesSummaryQuery request, CancellationToken ct)
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
            var filteredOrders = basis == "paid"
                ? await _db.Orders.AsNoTracking()
                    .Where(o => o.RestaurantId == request.RestaurantId
                        && o.Status == OrderStatus.PAID
                        && (o.PaidAt ?? o.UpdatedAt) >= rangeStartUtc
                        && (o.PaidAt ?? o.UpdatedAt) < rangeEndUtc)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderNumber,
                        o.Total,
                        o.PaymentMethod,
                        OccurredAtUtc = o.PaidAt ?? o.UpdatedAt
                    })
                    .ToListAsync(ct)
                : await _db.Orders.AsNoTracking()
                    .Where(o => o.RestaurantId == request.RestaurantId
                        && o.Status != OrderStatus.CANCELLED
                        && o.CreatedAt >= rangeStartUtc
                        && o.CreatedAt < rangeEndUtc)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderNumber,
                        o.Total,
                        o.PaymentMethod,
                        OccurredAtUtc = o.CreatedAt
                    })
                    .ToListAsync(ct);

            var paidOrdersCount = filteredOrders.Count;
            var totalSales = filteredOrders.Sum(o => o.Total);
            var averageTicket = paidOrdersCount > 0
                ? decimal.Round(totalSales / paidOrdersCount, 2)
                : 0m;

            var cashOrders = filteredOrders.Where(o => o.PaymentMethod == PaymentMethod.CASH).ToList();
            var cardOrders = filteredOrders.Where(o => o.PaymentMethod == PaymentMethod.CARD).ToList();
            var orders = filteredOrders
                .OrderByDescending(o => o.OccurredAtUtc)
                .Select(o => new SalesSummaryOrderItem(
                    OrderId: o.Id,
                    OrderNumber: o.OrderNumber,
                    Total: o.Total,
                    PaymentMethod: o.PaymentMethod?.ToString(),
                    OccurredAtUtc: o.OccurredAtUtc
                ))
                .ToList();

            return new GetOpsSalesSummaryResult(
                Period: period,
                Basis: basis,
                TimeZone: timeZone.Id,
                RangeStartUtc: rangeStartUtc,
                RangeEndUtc: rangeEndUtc,
                PaidOrdersCount: paidOrdersCount,
                TotalSales: totalSales,
                AverageTicket: averageTicket,
                Cash: new PaymentMethodSummary(
                    Amount: cashOrders.Sum(o => o.Total),
                    OrdersCount: cashOrders.Count
                ),
                Card: new PaymentMethodSummary(
                    Amount: cardOrders.Sum(o => o.Total),
                    OrdersCount: cardOrders.Count
                ),
                Orders: orders
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

