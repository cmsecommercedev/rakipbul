using System.Collections.Generic;

public class RakipbulSearchResponse
{
    public List<RakipbulPlayerDto> Player { get; set; } = new();
    // Diğer alanlar (team, club, vs.) eklenebilir
}

public class RakipbulPlayerDto
{
    public int Id { get; set; }
    public string Fullname { get; set; }
    public string Image { get; set; }
    public int Player_Id { get; set; }
    public int Number { get; set; }
    public string Foot { get; set; }
    public int Weight { get; set; }
    public int Height { get; set; }
    public string Facebook { get; set; }
    public string Twitter { get; set; }
    public string Instagram { get; set; }
    public string Value { get; set; }
    public string Image_Url { get; set; }
    public RakipbulPositionDto Position { get; set; }
    public RakipbulPositionDto Position2 { get; set; }
    public RakipbulPositionDto Position3 { get; set; }
    public RakipbulTeamDto Team { get; set; }  // ✔️ object yerine gerçek sınıf
}

public class RakipbulPositionDto
{
    public int Id { get; set; }
    public string Created_At { get; set; }
    public string Updated_At { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public int Order { get; set; }
}
