namespace Rakipbul.DTOs
{
    public class RegisterDeviceRequest
    {
        public string MacId { get; set; }

        public string DeviceToken { get; set; } = null!;

        public string Culture { get; set; } = "tr"; // tr / en
    }

}
