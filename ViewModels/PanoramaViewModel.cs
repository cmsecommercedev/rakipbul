using System;
using System.Collections.Generic;

public class PanoramaViewModel
{
    public List<RakipbulLeagueDto>? Leagues { get; set; }
    public int? SelectedLeagueId { get; set; }
    public string? SelectedSeason { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? YoutubeEmbedLink { get; set; }
    public string? LeagueName { get; set; }
    public string? LeagueDetails { get; set; }
    public List<RakipbulSeasonDto>? Seasons { get; set; }
}

public class PanoramaFormModel
{
    public int LeagueId { get; set; }
    public string? Season { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? YoutubeEmbedLink { get; set; }
}
