namespace QrCafe.Api.Auth
{
    public static class AuthConstants
    {
        public const string RestaurantIdClaim = "restaurantId";
        public const string EmailClaim = "email";
        public const string RoleClaim = "role";

        public const string PolicyStaffAny = "StaffAny";
        public const string PolicyAdminOnly = "AdminOnly";
        public const string PolicySuperAdminOnly = "SuperAdminOnly";
        public const string PolicyKitchenOrAdmin = "KitchenOrAdmin";
        public const string PolicyWaiterOrAdmin = "WaiterOrAdmin";
        public const string PolicyDeliveryOrAdmin = "DeliveryOrAdmin";
    }
}
