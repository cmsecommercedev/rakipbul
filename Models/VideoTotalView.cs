using System;

namespace Rakipbul.Models
{
    public class VideoTotalView
    {
        public int Id { get; set; }
        public string? VideoId { get; set; } // Harici video kimliği
        public int TotalViews { get; set; } // Toplam izlenme sayısı
        public string? EmbedCode { get; set; } // Video embed kodu
        public string? VideoUrl { get; set; } // Video URL'si
        public string? VideoImage { get; set; } // Video önizleme resmi
    }
}