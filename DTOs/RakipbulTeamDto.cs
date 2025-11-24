public class RakipbulTeamDto
{
    public int Id { get; set; }
    public string Created_At { get; set; }
    public string Updated_At { get; set; }
    public string Name { get; set; }
    public string Facebook { get; set; }
    public string Twitter { get; set; }
    public string Instagram { get; set; }
    public string Status { get; set; }
    public string Image { get; set; }
    public int Captain_Id { get; set; }
    public int District_Id { get; set; }
    public int League_Id { get; set; }
    public int Province_Id { get; set; }
    public int Tactic_Id { get; set; }
    public string Cover_Image { get; set; }
    public string Image_Url { get; set; }

    public RakipbulTeamLeagueDto League { get; set; }
}
public class RakipbulTeamLeagueDto
{
    public int Id { get; set; }
    public string Created_At { get; set; }
    public string Updated_At { get; set; }
    public string Name { get; set; }
    public string Facebook { get; set; }
    public string Twitter { get; set; }
    public string Instagram { get; set; }
    public bool Active { get; set; }

    public RakipbulProvinceDto Province { get; set; }
    public object Season { get; set; } // season null geliyor → şimdilik object
} 