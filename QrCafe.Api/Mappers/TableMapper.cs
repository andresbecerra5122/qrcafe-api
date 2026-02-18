using QrCafe.Api.Dto.Tables;
using QrCafe.Application.Tables.Queries.ResolveTable;

namespace QrCafe.Api.Mappers
{
    public static class TableMapper
    {
        public static TablePublicDto ToDto(this ResolveTableResult result)
        {
            return new TablePublicDto(
                result.Number,
                result.Token
            );
        }
    }
}
