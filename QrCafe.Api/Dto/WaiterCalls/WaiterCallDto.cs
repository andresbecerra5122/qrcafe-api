namespace QrCafe.Api.Dto.WaiterCalls
{
    public record WaiterCallDto(
        Guid Id,
        int? TableNumber,
        string Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? AttendedAt
    );
}
