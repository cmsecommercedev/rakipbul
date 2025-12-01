namespace Rakipbul.Models
{
    public class DeviceTopicSubscription
    {
        public int Id { get; set; }

        public string Token { get; set; } = null!;

        public string Topic { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
