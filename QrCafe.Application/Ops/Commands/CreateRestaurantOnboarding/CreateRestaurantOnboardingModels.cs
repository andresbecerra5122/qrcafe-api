namespace QrCafe.Application.Ops.Commands.CreateRestaurantOnboarding
{
    public record CreateRestaurantOnboardingStaffInput(
        string FullName,
        string Email,
        string Password,
        string Role
    );

    public record CreateRestaurantOnboardingProductInput(
        string Name,
        string? Description,
        decimal Price,
        bool IsActive,
        bool IsAvailable,
        int Sort,
        string? ImageUrl
    );

    public record CreateRestaurantOnboardingCategoryInput(
        string Name,
        int Sort,
        IReadOnlyList<CreateRestaurantOnboardingProductInput> Products
    );

    public record CreateRestaurantOnboardingInput(
        string Name,
        string Slug,
        string CountryCode,
        string Currency,
        string TimeZone,
        decimal TaxRate,
        bool EnableDineIn,
        bool EnableDelivery,
        bool EnableDeliveryCash,
        bool EnableDeliveryCard,
        string AdminFullName,
        string AdminEmail,
        string AdminPassword,
        int InitialTablesCount,
        IReadOnlyList<CreateRestaurantOnboardingStaffInput> StaffUsers,
        IReadOnlyList<CreateRestaurantOnboardingCategoryInput> Categories
    );

    public record CreateRestaurantOnboardingResult(
        Guid RestaurantId,
        string Name,
        string Slug,
        string AdminEmail,
        int TablesCreated,
        int CategoriesCreated,
        int ProductsCreated,
        int StaffUsersCreated
    );
}
