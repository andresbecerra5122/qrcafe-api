namespace QrCafe.Api.Dto.WaiterCalls
{
    public record CreateWaiterCallRequestDto(Guid RestaurantId, string? TableToken);
    public record CreateWaiterCallResponseDto(Guid WaiterCallId);
}
