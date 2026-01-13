using System;

public class MobileVideoStat
{
    public int Id { get; set; }
    public string VideoId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public int UnlikeCount { get; set; }
    public int ViewCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
