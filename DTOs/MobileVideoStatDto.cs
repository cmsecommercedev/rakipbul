public class MobileVideoStatDto
{
    public string VideoId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int LikeCount { get; set; }
    public int UnlikeCount { get; set; }
    public int ViewCount { get; set; }
}