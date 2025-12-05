using System;

namespace Rakipbul.Models
{
    /// <summary>
    /// Harici (mobil) videolar için kullanıcı bazlı beğenileri tutar.
    /// VideoId tamamen harici sistemden gelen string bir anahtardır.
    /// </summary>
    public class VideoLike
    {
        public int Id { get; set; }

        /// <summary>
        /// Harici video kimliği (ör: mobil uygulamadan gelen string id).
        /// </summary>
        public string VideoId { get; set; } = string.Empty;

        /// <summary>
        /// Identity kullanıcı anahtarı (AspNetUsers.Id) veya mobilde tuttuğunuz user id.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

