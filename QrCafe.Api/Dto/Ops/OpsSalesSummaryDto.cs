namespace QrCafe.Api.Dto.Ops
{
    public record OpsPaymentMethodSummaryDto(decimal Amount, int OrdersCount);
    public record OpsSalesSummaryOrderDto(
        Guid OrderId,
        long OrderNumber,
        decimal Total,
        string? PaymentMethod,
        DateTimeOffset OccurredAtUtc
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
        OpsPaymentMethodSummaryDto Cash,
        OpsPaymentMethodSummaryDto Card,
        IReadOnlyList<OpsSalesSummaryOrderDto> Orders
    );
}

