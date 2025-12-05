using System;

namespace Rakipbul.Models
{
    /// <summary>
    /// Harici videolar için toplam izlenme sayısını tutar.
    /// </summary>
    public class VideoStat
    {
        public int Id { get; set; }

        /// <summary>
        /// Harici video kimliği (ör: mobil uygulamadan gelen string id).
        /// </summary>
        public string VideoId { get; set; } = string.Empty;

        /// <summary>
        /// Toplam izlenme sayısı.
        /// </summary>
        public int ViewCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}


