using System.ComponentModel.DataAnnotations;

namespace Rakipbul.Models
{
    /// <summary>
    /// Takım kadro görselleri
    /// </summary>
    public class TeamSquadImage
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Rakipbul API'dan gelen takım ID'si
        /// </summary>
        [Required]
        public int TeamId { get; set; }

        /// <summary>
        /// Cloudflare R2'deki dosya anahtarı (key)
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string ImageKey { get; set; } = string.Empty;

        /// <summary>
        /// Görselin tam URL'i
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Oluşturulma tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
