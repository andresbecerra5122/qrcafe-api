namespace QrCafe.Api.Dto.Ops
{
    public record OpsPaymentMethodSummaryDto(decimal Amount, int OrdersCount);
    public record OpsSalesSummaryOrderDto(
        Guid OrderId,
        long OrderNumber,
        decimal Total,
        string? PaymentMethodCode,
        string? PaymentMethodLabel,
        DateTimeOffset OccurredAtUtc
    );

    public record OpsSalesPaymentMethodBreakdownDto(
        string MethodCode,
        string MethodLabel,
        decimal Amount,
        int OrdersCount
    );

    public record OpsSalesSummaryDto(
        string Period,
        string Basis,
        string TimeZone,
        DateTimeOffset RangeStartUtc,
        DateTimeOffset RangeEndUtc,
        int PaidOrdersCount,
        decimal TotalSales,
        decimal AverageTicket,
        IReadOnlyList<OpsSalesPaymentMethodBreakdownDto> PaymentMethods,
        IReadOnlyList<OpsSalesSummaryOrderDto> Orders
    );
}

