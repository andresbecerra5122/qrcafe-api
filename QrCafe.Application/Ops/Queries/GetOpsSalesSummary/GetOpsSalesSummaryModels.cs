using System;
using System.Collections.Generic;

namespace QrCafe.Application.Ops.Queries.GetOpsSalesSummary
{
    public record PaymentMethodSummary(decimal Amount, int OrdersCount);
    public record SalesSummaryOrderItem(
        Guid OrderId,
        long OrderNumber,
        decimal Total,
        string? PaymentMethod,
        DateTimeOffset OccurredAtUtc
    );

    public record GetOpsSalesSummaryResult(
        string Period,
        string Basis,
        string TimeZone,
        DateTimeOffset RangeStartUtc,
        DateTimeOffset RangeEndUtc,
        int PaidOrdersCount,
        decimal TotalSales,
        decimal AverageTicket,
        PaymentMethodSummary Cash,
        PaymentMethodSummary Card,
        IReadOnlyList<SalesSummaryOrderItem> Orders
    );
}

