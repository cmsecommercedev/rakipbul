public class MobileVideoStatDto
{
    public string VideoId { get; set; }
    public string UserId { get; set; }
    public bool Like { get; set; } // Kullanıcı beğenmiş mi?
    public bool Unlike { get; set; } // Kullanıcı beğenmemiş mi?

    // Yeni alanlar
    public string? EmbedCode { get; set; }
    public string? VideoUrl { get; set; }
    public string? VideoImage { get; set; }
}