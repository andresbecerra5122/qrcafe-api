namespace QrCafe.Api.Dto.Ops
{
    public class BulkCreateProductsRequestDto
    {
        public List<BulkCreateProductItemDto> Products { get; set; } = [];
    }

    public class BulkCreateProductItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public string? PrepStation { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Sort { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
