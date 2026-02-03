using QrCafe.Api.Dto.Ops;
using QrCafe.Application.Ops.Queries.GetOpsOrders;

namespace QrCafe.Api.Mappers
{
    public static class OpsOrdersMapper
    {
        public static OpsOrderListItemDto ToDto(GetOpsOrdersItem r) => new(
            r.OrderId, r.OrderType, r.TableNumber, r.CustomerName, r.Status, r.Currency, r.Total, r.CreatedAt
        );
    }
}

