namespace QrCafe.Api.Dto.Ops
{
    public class UpdateRestaurantSettingsRequestDto
    {
        public bool? EnableDineIn { get; set; }
        public bool? EnableDelivery { get; set; }
        public bool? EnableDeliveryCash { get; set; }
        public bool? EnableDeliveryCard { get; set; }
        public bool? EnableKitchenBarSplit { get; set; }
    }
}
