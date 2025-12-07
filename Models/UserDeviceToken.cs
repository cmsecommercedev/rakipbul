namespace Rakipbul.Models
{
    public class UserDeviceToken
    {
        public int Id { get; set; }

        public string MacId { get; set; }

        public string Token { get; set; } = null!;

        public string Culture { get; set; } = "tr"; // tr veya en

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
