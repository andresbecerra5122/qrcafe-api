namespace QrCafe.Api.Dto.Menu
{
    public record CategoryDto(
        Guid Id,
        string Name,
        int Sort
    );
}
