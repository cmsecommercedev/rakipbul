using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RakipBul.Models
{
    public class FavouritePlayers
    {
        [Key]
        public int FavouritePlayerID { get; set; }

        [Required]
        public int PlayerID { get; set; }
        
        [MaxLength(255)]
        public string? PlayerName { get; set; }
        
        [MaxLength(500)]
        public string? PlayerImageUrl { get; set; }
        
        [Required]
        public string UserToken { get; set; }
        public string MacID { get; set; }
        public string? Culture { get; set; }
    }
}