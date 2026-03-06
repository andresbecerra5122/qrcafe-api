namespace QrCafe.Api.Dto.Ops
{
    public class UpdateStaffUserRequestDto
    {
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
