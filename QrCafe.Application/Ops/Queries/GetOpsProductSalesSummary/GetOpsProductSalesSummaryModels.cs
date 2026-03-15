using System;
using System.Collections.Generic;

namespace QrCafe.Application.Ops.Queries.GetOpsProductSalesSummary
{
    public record ProductSalesSummaryItem(
        Guid ProductId,
        string ProductName,
        int QtySold,
        decimal Revenue
    );

    public record GetOpsProductSalesSummaryResult(
        string Period,
        string Basis,
        string TimeZone,
        DateTimeOffset RangeStartUtc,
        DateTimeOffset RangeEndUtc,
        int TotalItemsSold,
        decimal TotalRevenue,
        IReadOnlyList<ProductSalesSummaryItem> Products
    );
}

