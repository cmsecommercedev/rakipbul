using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Rakipbul.DTOs
{
    /// <summary>
    /// Takım kadro görseli yükleme DTO'su
    /// </summary>
    public class TeamSquadImageUploadDto
    {
        /// <summary>
        /// Yüklenecek görsel dosyası
        /// </summary>
        [Required(ErrorMessage = "Görsel dosyası gereklidir.")]
        public IFormFile Image { get; set; } = null!;

        /// <summary>
        /// Rakipbul API'dan gelen takım ID'si
        /// </summary>
        [Required(ErrorMessage = "TeamId gereklidir.")]
        [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir TeamId giriniz.")]
        public int TeamId { get; set; }
    }
}
