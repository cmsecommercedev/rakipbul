namespace Rakipbul.Models
{
    public class DeviceToken
    {
        public int Id { get; set; }

        // Firebase Token
        public string Token { get; set; } = null!;

        // Kullanıcıya bağlı ise (opsiyonel)
        public string? UserId { get; set; }

        // iOS / Android / Web
        public string Platform { get; set; } = null!;

        // Token son ne zaman güncellendi
        public DateTime UpdatedAt { get; set; }

        // Token ilk ne zaman kaydedildi
        public DateTime CreatedAt { get; set; }


        // Hangi topiclere abone (sadece local kayıt — opsiyonel)
        public string? SubscribedTopic { get; set; }
    }
}
