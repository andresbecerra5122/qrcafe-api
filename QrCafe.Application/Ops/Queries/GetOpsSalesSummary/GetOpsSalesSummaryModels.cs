using System;
using System.Collections.Generic;

namespace QrCafe.Application.Ops.Queries.GetOpsSalesSummary
{
    public record PaymentMethodSummary(decimal Amount, int OrdersCount);
    public record PaymentMethodBreakdown(string MethodCode, string MethodLabel, decimal Amount, int OrdersCount);
    public record SalesSummaryOrderItem(
        Guid OrderId,
        long OrderNumber,
        decimal Total,
        string? PaymentMethodCode,
        string? PaymentMethodLabel,
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
        IReadOnlyList<PaymentMethodBreakdown> PaymentMethods,
        IReadOnlyList<SalesSummaryOrderItem> Orders
    );
}

