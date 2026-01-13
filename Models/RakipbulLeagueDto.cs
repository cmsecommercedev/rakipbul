public class RakipbulLeagueDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Facebook { get; set; }
    public string Twitter { get; set; }
    public string Instagram { get; set; }
    public RakipbulSeasonDto Season { get; set; }
    public RakipbulProvinceDto Province { get; set; }
}

public class RakipbulSeasonDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool Active { get; set; }
    public int Year { get; set; }
    public string Start_Date { get; set; }
    public string End_Date { get; set; }
    public string Transfer_Start_Date { get; set; }
    public string Transfer_End_Date { get; set; }
    public int Allowed_Transfer_Count { get; set; }
    public int Locking_Match_Count { get; set; }
    public int League_Id { get; set; }
    public bool Closed { get; set; }
    public string Fullname { get; set; }
    public string Type { get; set; }
}
public class RakipbulProvinceDto
{
    public int Id { get; set; }
    public string Created_At { get; set; }
    public string Updated_At { get; set; }
    public string Name { get; set; }
    public string Slug { get; set; }
    public int Plate_Code { get; set; }
}
