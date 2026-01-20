using System.Collections.Generic;

public class RakipbulMatchDto
{
    public int Id { get; set; }
    public string? Created_At { get; set; }
    public string? Updated_At { get; set; }
    public string? Date { get; set; }
    public string? Time { get; set; }
    public bool Completed { get; set; }
    public int Team1_Goal { get; set; }
    public int Team2_Goal { get; set; }
    public int? Coordinator_Id { get; set; }
    public int? Ground_Id { get; set; }
    public int? Referee_Id { get; set; }
    public int Season_Id { get; set; }
    public int Team1_Id { get; set; }
    public int Team2_Id { get; set; }
    public int Team1_Point { get; set; }
    public int Team2_Point { get; set; }
    public int? Team1_Point_Before { get; set; }
    public int? Team2_Point_Before { get; set; }
    public string? Completed_At { get; set; }
    public string? Completed_Status { get; set; }
    public string? Sendsms { get; set; }
    public int App_Status { get; set; }
    public int Season { get; set; }
    public string? Video_Status { get; set; }
    public RakipbulTeamDetailDto? Team1 { get; set; }
    public RakipbulTeamDetailDto? Team2 { get; set; }
    public RakipbulGroundDto? Ground { get; set; }

    // Helper property for display
    public string MatchDisplay => $"{Team1?.Name ?? "Takım 1"} vs {Team2?.Name ?? "Takım 2"}";
}

public class RakipbulTeamDetailDto
{
    public int Id { get; set; }
    public string? Created_At { get; set; }
    public string? Updated_At { get; set; }
    public string? Name { get; set; }
    public string? Facebook { get; set; }
    public string? Twitter { get; set; }
    public string? Instagram { get; set; }
    public string? Status { get; set; }
    public string? Image { get; set; }
    public int? Captain_Id { get; set; }
    public int? District_Id { get; set; }
    public int? League_Id { get; set; }
    public int? Province_Id { get; set; }
    public int? Tactic_Id { get; set; }
    public string? Cover_Image { get; set; }
    public string? Image_Url { get; set; }
    public RakipbulLeagueDto? League { get; set; }
}

public class RakipbulGroundDto
{
    public int Id { get; set; }
    public string? Created_At { get; set; }
    public string? Updated_At { get; set; }
    public string? Name { get; set; }
    public string? Slug { get; set; }
    public string? Phone { get; set; }
    public string? Phone2 { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? Latitude { get; set; }
    public string? Longitude { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public int Capacity { get; set; }
    public int Width { get; set; }
    public int Length { get; set; }
    public int? District_Id { get; set; }
    public int? Province_Id { get; set; }
}
