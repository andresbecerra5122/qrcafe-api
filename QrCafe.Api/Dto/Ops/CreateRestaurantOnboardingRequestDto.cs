using System.ComponentModel.DataAnnotations;

namespace QrCafe.Api.Dto.Ops
{
    public class CreateRestaurantOnboardingRequestDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Slug { get; set; } = string.Empty;
        public string CountryCode { get; set; } = "CO";
        public string Currency { get; set; } = "COP";
        public string TimeZone { get; set; } = "America/Bogota";
        public decimal TaxRate { get; set; } = 0m;
        public bool EnableDineIn { get; set; } = true;
        public bool EnableDelivery { get; set; } = false;
        public bool EnableDeliveryCash { get; set; } = true;
        public bool EnableDeliveryCard { get; set; } = true;

        [Required]
        public string AdminFullName { get; set; } = string.Empty;
        [Required]
        public string AdminEmail { get; set; } = string.Empty;
        [Required]
        public string AdminPassword { get; set; } = string.Empty;

        public int InitialTablesCount { get; set; } = 0;

        public List<OnboardingStaffUserDto> StaffUsers { get; set; } = new();
        public List<OnboardingCategoryDto> Categories { get; set; } = new();
    }

    public class OnboardingStaffUserDto
    {
        [Required]
        public string FullName { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = string.Empty;
    }

    public class OnboardingCategoryDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public int Sort { get; set; } = 0;
        public List<OnboardingProductDto> Products { get; set; } = new();
    }

    public class OnboardingProductDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsAvailable { get; set; } = true;
        public int Sort { get; set; } = 0;
        public string? ImageUrl { get; set; }
    }

    public class CreateRestaurantOnboardingResponseDto
    {
        public Guid RestaurantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public int TablesCreated { get; set; }
        public int CategoriesCreated { get; set; }
        public int ProductsCreated { get; set; }
        public int StaffUsersCreated { get; set; }
    }
}
