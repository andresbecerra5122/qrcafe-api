namespace QrCafe.Api.Dto.Ops
{
    public record OpsProductSalesSummaryItemDto(
        Guid ProductId,
        string ProductName,
        int QtySold,
        decimal Revenue
    );

    public record OpsProductSalesSummaryDto(
        string Period,
        string Basis,
        string TimeZone,
        DateTimeOffset RangeStartUtc,
        DateTimeOffset RangeEndUtc,
        int TotalItemsSold,
        decimal TotalRevenue,
        decimal TipTotal,
        IReadOnlyList<OpsProductSalesSummaryItemDto> Products
    );
}

