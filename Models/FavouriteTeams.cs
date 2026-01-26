using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RakipBul.Models
{
    public class FavouriteTeams
    {
        [Key]
        public int FavouriteTeamID { get; set; }

        [Required]
        public int TeamID { get; set; }
        
        [MaxLength(255)]
        public string? TeamName { get; set; }
        
        [MaxLength(500)]
        public string? TeamImageUrl { get; set; }
        
        [Required]
        public string UserToken { get; set; }
        public string MacID { get; set; }
        public string? Culture { get; set; }
    }
}