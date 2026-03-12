namespace QrCafe.Api.Dto.Ops
{
    public class ChangeMyPasswordRequestDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
