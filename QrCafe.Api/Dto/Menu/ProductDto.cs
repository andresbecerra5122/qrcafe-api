namespace QrCafe.Api.Dto.Menu
{
    public record ProductDto(
        Guid Id,
        Guid? CategoryId,
        string Name,
        string? Description,
        decimal Price,
        bool IsAvailable,
        int Sort,
        string? ImageUrl
    );
}
