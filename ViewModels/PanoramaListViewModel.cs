using Rakipbul.Models;

namespace Rakipbul.ViewModels
{
    public class PanoramaListViewModel
    {
        public List<RakipbulLeagueDto> Leagues { get; set; } = new();
        public List<RakipbulSeasonDto> Seasons { get; set; } = new();

        public PanoramaFilterModel Filter { get; set; } = new();

        public List<PanoramaItemDto> Items { get; set; } = new();
    }
    public class PanoramaFilterModel
    {
        public PanoramaCategory? Category { get; set; }

        public int? LeagueId { get; set; }
        public int? SeasonId { get; set; }
        public string? Season { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class PanoramaItemDto
    {
        public int Id { get; set; }
        public PanoramaCategory Category { get; set; }

        public string Title { get; set; }
        public string? YoutubeEmbedLink { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int? PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public string? PlayerImageUrl { get; set; }
        public string? PlayerPosition { get; set; }

        public int? TeamId { get; set; }
        public string? TeamName { get; set; }
        public string? TeamImageUrl { get; set; }

        public int? LeagueId { get; set; }
        public string? LeagueName { get; set; }
        public string? ProvinceName { get; set; }
    }



}
