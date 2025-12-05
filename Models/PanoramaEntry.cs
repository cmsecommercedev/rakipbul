namespace Rakipbul.Models
{
    public class PanoramaEntry
    {
        public int Id { get; set; }

        public PanoramaCategory Category { get; set; }

        public string Title { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? YoutubeEmbedLink { get; set; }

        /// <summary>
        /// Videonun toplam izlenme sayısı.
        /// </summary>
        public int ViewCount { get; set; }

        // Player Info
        public int? PlayerId { get; set; }
        public int? SeasonId { get; set; }
        public string? PlayerName { get; set; }
        public string? PlayerImageUrl { get; set; }
        public string? PlayerPosition { get; set; }

        // Team Info
        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? TeamImageUrl { get; set; }

        // League Info
        public int? LeagueId { get; set; }
        public string? LeagueName { get; set; }
        public string? ProvinceName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
    public enum PanoramaCategory
    {
        Panorama = 1,
        Goals = 2
    }


}
