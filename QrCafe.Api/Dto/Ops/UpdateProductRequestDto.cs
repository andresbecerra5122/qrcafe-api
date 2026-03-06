namespace QrCafe.Api.Dto.Ops
{
    public class UpdateProductRequestDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
        public int? Sort { get; set; }
        public bool? IsAvailable { get; set; }
        public bool? IsActive { get; set; }
    }
}
