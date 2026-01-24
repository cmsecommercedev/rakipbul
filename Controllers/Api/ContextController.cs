using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Rakipbul.DTOs;
using Rakipbul.Models;
using RakipBul.Attributes; // Kullanıcının kimliğini almak için (opsiyonel)
using RakipBul.Data;
using RakipBul.Models;
using RakipBul.Models.Dtos;// NotificationManager için
using System;

namespace RakipBul.Controllers.Api // Namespace'i kontrol edin
{
    //[ApiKeyAuth]
    [Route("api/[controller]")]
    [ApiController]
    public class ContextController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContextController> _logger;
        private readonly IMemoryCache _cache;
        private readonly CloudflareR2Manager _r2Manager;

        public ContextController(ApplicationDbContext context, ILogger<ContextController> logger, IMemoryCache cache, CloudflareR2Manager r2Manager)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
            _r2Manager = r2Manager;
        }

        // GET: /api/news/list?culture=tr
        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<MatchNewsDto>>> GetMatchNewsList([FromQuery] string culture = "tr")
        {
            // Haberleri içerik ve fotoğrafları ile birlikte çek
            var items = await _context.MatchNews
                .AsNoTracking()
                .Where(m => m.Published)
                .Include(m => m.Photos)
                .Include(m => m.Contents)
                .OrderByDescending(m => m.CreatedDate)
                .Select(m => new MatchNewsDto
                {
                    Id = m.Id,
                    MatchNewsMainPhoto = m.MatchNewsMainPhoto ?? string.Empty,
                    CreatedDate = m.CreatedDate,
                    // Culture'a göre tekil içerik alanları
                    Title = m.Contents.Where(c => c.Culture == culture).Select(c => c.Title).FirstOrDefault(),
                    Subtitle = m.Contents.Where(c => c.Culture == culture).Select(c => c.Subtitle).FirstOrDefault(),
                    DetailsTitle = m.Contents.Where(c => c.Culture == culture).Select(c => c.DetailsTitle).FirstOrDefault(),
                    Details = m.Contents.Where(c => c.Culture == culture).Select(c => c.Details).FirstOrDefault(),
                    // Fotoğraflar
                    Photos = m.Photos
                        .Select(p => new MatchNewsPhotoDto
                        {
                            Id = p.Id,
                            PhotoUrl = p.PhotoUrl
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: /api/context/static?key=someKey
        [HttpGet("static")]
        public async Task<ActionResult> GetStatic([FromQuery] string? key = null)
        {
            var query = _context.StaticKeyValues.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(key))
            {
                var item = await query
                    .Where(s => s.Key == key)
                    .Select(s => new { s.Key, s.Value, s.UpdatedAt })
                    .FirstOrDefaultAsync();

                if (item == null) return NotFound();
                return Ok(item);
            }

            var items = await query
                .OrderByDescending(s => s.UpdatedAt)
                .Select(s => new { s.Key, s.Value, s.UpdatedAt })
                .ToListAsync();

            return Ok(items);
        }

        // GET: /api/context/photos?category=2024
        [HttpGet("photos")]
        public async Task<ActionResult> GetPhotos([FromQuery] string? category = null)
        {
            var query = _context.PhotoGalleries.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            var items = await query
                .OrderByDescending(p => p.UploadedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Category,
                    p.FileName,
                    p.FilePath,
                    p.UploadedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("stories")]
        public async Task<ActionResult<List<StoryDto>>> GetStories([FromQuery] string? type = null)
        {
            string? normType = type?.Trim().ToLower();
            if (!string.IsNullOrEmpty(normType) && normType != "image" && normType != "video")
            {
                return BadRequest(new { message = "type 'image' veya 'video' olmalıdır." });
            }

            var last24Hours = DateTime.UtcNow.AddDays(-240);

            IQueryable<Story> query = _context.Stories
                .AsNoTracking()
                .Where(s => s.Published && s.UpdatedAt >= last24Hours) // Son 24 saat filtresi
                .Include(s => s.Contents);

            if (!string.IsNullOrEmpty(normType))
            {
                query = query.Where(s => s.Contents.Any(c =>
                    !string.IsNullOrEmpty(c.ContentType) &&
                    c.ContentType.StartsWith(normType)));
            }

            query = query.OrderByDescending(s => s.UpdatedAt);

            var items = await query
                .Select(s => new StoryDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    StoryImage = s.StoryImage,
                    Published = s.Published,
                    UpdatedAt = s.UpdatedAt,
                    Contents = s.Contents
                        .Where(c => string.IsNullOrEmpty(normType) ||
                            (!string.IsNullOrEmpty(c.ContentType) && c.ContentType.StartsWith(normType)))
                        .OrderBy(c => c.DisplayOrder)
                        .Select(c => new StoryContentDto
                        {
                            Id = c.Id,
                            MediaUrl = c.MediaUrl,
                            ContentType = c.ContentType,
                            DisplayOrder = c.DisplayOrder
                        })
                        .ToList(),
                    Type = s.Contents.Any(c => c.ContentType.StartsWith("video"))
                                ? "video"
                                : "image"
                })
                .ToListAsync();

            return Ok(items);
        }


        // GET: /api/context/richstatic?category=flags&culture=tr&season=2024
        [HttpGet("richstatic")]
        public async Task<ActionResult> GetRichStatic([FromQuery] string? category = null, [FromQuery] string? culture = null, [FromQuery] bool? published = true, [FromQuery] string? season = null)
        {
            var query = _context.RichStaticContents
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Season)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(x => x.Category != null && x.Category.Code == category);
            }
            if (!string.IsNullOrWhiteSpace(culture))
            {
                query = query.Where(x => x.Culture == culture);
            }
            if (!string.IsNullOrWhiteSpace(season))
            {
                query = query.Where(x => x.Season != null && x.Season.Name == season);
            }
            if (published.HasValue)
            {
                query = query.Where(x => x.Published == published.Value);
            }

            var items = await query
                .OrderByDescending(x => x.UpdatedAt)
                .Select(x => new
                {
                    x.Id,
                    CategoryCode = x.Category != null ? x.Category.Code : null,
                    CategoryName = x.Category != null ? x.Category.Name : null,
                    SeasonId = x.SeasonId,
                    SeasonName = x.Season != null ? x.Season.Name : null,
                    x.Culture,
                    x.MediaUrl,
                    x.ProfileImageUrl,
                    x.EmbedVideoUrl,
                    x.Text,
                    x.AltText,
                    x.Published,
                    x.UpdatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("app-settings")]
        public async Task<ActionResult<Settings>> GetAppSettings()
        {
            try
            {
                var settings = await _context.Settings
                    .OrderByDescending(s => s.LastUpdated)
                    .FirstOrDefaultAsync();

                if (settings == null)
                {
                    _logger.LogWarning("Ayarlar bulunamadı");
                    return NotFound(new { error = "Ayarlar bulunamadı" });
                }

                return Ok(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Uygulama ayarları yüklenirken hata oluştu");
                return StatusCode(500, new { error = "Uygulama ayarları yüklenirken bir hata oluştu" });
            }
        }

        [HttpGet("panorama/today")]
        public async Task<ActionResult<IEnumerable<PanoramaDto>>> GetTodayPanoramaEntries([FromQuery] int leagueId, [FromQuery] int seasonId, [FromQuery] PanoramaCategory category)
        {
            if (leagueId <= 0)
                return BadRequest(new { message = "Geçerli bir LeagueId gönderilmelidir." });

            if (seasonId <= 0)
                return BadRequest(new { message = "Geçerli bir SeasonId gönderilmelidir." });

            if (!Enum.IsDefined(typeof(PanoramaCategory), category))
                return BadRequest(new { message = "Geçersiz kategori. (1=Panorama, 2=Goals)" });

            var today = DateTime.UtcNow.Date;

            var panoramas = await _context.PanoramaEntries
                .AsNoTracking()
                .Where(p =>
                    p.LeagueId == leagueId &&
                    p.SeasonId == seasonId &&
                    p.Category == category &&     // ✔ kategori filtresi
                    p.StartDate <= today &&
                    p.EndDate >= today
                )
                .Select(p => new PanoramaDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    YoutubeEmbedLink = p.YoutubeEmbedLink,

                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName,
                    PlayerImageUrl = p.PlayerImageUrl,
                    PlayerPosition = p.PlayerPosition,

                    TeamId = p.TeamId,
                    TeamName = p.TeamName,
                    TeamImageUrl = p.TeamImageUrl,

                    LeagueId = p.LeagueId,
                    LeagueName = p.LeagueName,
                    ProvinceName = p.ProvinceName,

                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            if (!panoramas.Any())
            {
                return NotFound(new
                {
                    message = "Bu lig, sezon ve kategori için bugünün tarih aralığında kayıt bulunamadı."
                });
            }

            return Ok(panoramas);
        }


        [HttpPost("video-stat")]
        public async Task<IActionResult> UpdateVideoStat([FromBody] MobileVideoStatDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.VideoId) || string.IsNullOrWhiteSpace(dto.UserId))
                return BadRequest(new { message = "VideoId ve UserId zorunludur." });

            // Kullanıcı bazlı istatistik
            var stat = await _context.MobileVideoStats
                .FirstOrDefaultAsync(x => x.VideoId == dto.VideoId && x.UserId == dto.UserId);

            if (stat == null)
            {
                stat = new MobileVideoStat
                {
                    VideoId = dto.VideoId,
                    UserId = dto.UserId,
                    LikeCount = dto.Like ? 1 : 0,
                    UnlikeCount = dto.Unlike ? 1 : 0,
                    ViewCount = 1
                };
                _context.MobileVideoStats.Add(stat);
            }
            else
            {
                // Like geldiyse Unlike sıfırlanacak
                if (dto.Like)
                {
                    stat.LikeCount = 1;
                    stat.UnlikeCount = 0;
                }
                // Unlike geldiyse Like sıfırlanacak
                else if (dto.Unlike)
                {
                    stat.LikeCount = 0;
                    stat.UnlikeCount = 1;
                }
                else
                {
                    // Hiçbiri gelmediyse tamamen sıfırla
                    stat.LikeCount = 0;
                    stat.UnlikeCount = 0;
                }

                stat.UpdatedAt = DateTime.UtcNow;
            }

            // Video toplam izlenme & metadata
            var totalView = await _context.VideoTotalView
                .FirstOrDefaultAsync(x => x.VideoId == dto.VideoId); 

            await _context.SaveChangesAsync();

            var totalStats = await _context.MobileVideoStats
                .Where(x => x.VideoId == dto.VideoId)
                .GroupBy(x => x.VideoId)
                .Select(g => new
                {
                    TotalLikes = g.Sum(s => s.LikeCount),
                    TotalUnlikes = g.Sum(s => s.UnlikeCount)
                })
                .FirstOrDefaultAsync();

            // Yanıt
            return Ok(new
            {
                stat.VideoId,
                stat.UserId,
                TotalViews = totalView != null ? totalView.TotalViews : 0,
                TotalLikes = totalStats?.TotalLikes ?? stat.LikeCount,
                TotalUnlikes = totalStats?.TotalUnlikes ?? stat.UnlikeCount,
                totalView?.EmbedCode,
                totalView?.VideoUrl,
                totalView?.VideoImage
            });
        }


        [HttpGet("video-stat")]
        public async Task<IActionResult> GetVideoStat([FromQuery] string videoId,[FromQuery] string userId,[FromQuery] string embedCode,[FromQuery] string videoUrl,[FromQuery] string videoImage)
        {
            if (string.IsNullOrWhiteSpace(videoId) || string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { message = "VideoId ve UserId zorunludur." });

            // Kullanıcı bazlı istatistik
            var stat = await _context.MobileVideoStats
                .FirstOrDefaultAsync(x => x.VideoId == videoId && x.UserId == userId);

            if (stat == null)
            {
                stat = new MobileVideoStat
                {
                    VideoId = videoId,
                    UserId = userId,
                    LikeCount = 0,
                    UnlikeCount = 0,
                    ViewCount = 1
                };
                _context.MobileVideoStats.Add(stat);
            }
            else
            {
                stat.ViewCount++;
                stat.UpdatedAt = DateTime.UtcNow;
            }

            // Toplam izlenme + metadata
            var totalView = await _context.VideoTotalView
                .FirstOrDefaultAsync(x => x.VideoId == videoId);

            if (totalView == null)
            {
                totalView = new VideoTotalView
                {
                    VideoId = videoId,
                    TotalViews = 1,
                    EmbedCode = embedCode,
                    VideoUrl = videoUrl,
                    VideoImage = videoImage
                };
                _context.VideoTotalView.Add(totalView);
            }
            else
            {
                totalView.TotalViews++;

                // GET isteğinden metadata geldiyse güncelle
                if (!string.IsNullOrWhiteSpace(embedCode))
                    totalView.EmbedCode = embedCode;

                if (!string.IsNullOrWhiteSpace(videoUrl))
                    totalView.VideoUrl = videoUrl;

                if (!string.IsNullOrWhiteSpace(videoImage))
                    totalView.VideoImage = videoImage;
            }

            await _context.SaveChangesAsync();

            // Toplam istatistikler
            var totalStats = await _context.MobileVideoStats
                .Where(x => x.VideoId == videoId)
                .GroupBy(x => x.VideoId)
                .Select(g => new
                {
                    TotalLikes = g.Sum(s => s.LikeCount),
                    TotalUnlikes = g.Sum(s => s.UnlikeCount)
                })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                stat.VideoId,
                stat.UserId,
                UserLikes = stat.LikeCount,
                UserUnlikes = stat.UnlikeCount,
                TotalViews = totalView.TotalViews,
                TotalLikes = totalStats?.TotalLikes ?? stat.LikeCount,
                TotalUnlikes = totalStats?.TotalUnlikes ?? stat.UnlikeCount,
                totalView.EmbedCode,
                totalView.VideoUrl,
                totalView.VideoImage
            });
        }

        [HttpGet("video-most-viewed")]
        public async Task<IActionResult> GetMostViewedVideos([FromQuery] int top = 10)
        {
            if (top <= 0)
                top = 10;

            var videos = await _context.VideoTotalView
                .OrderByDescending(v => v.TotalViews)
                .Take(top)
                .Select(v => new
                {
                    v.VideoId,
                    v.TotalViews,
                    v.EmbedCode,
                    v.VideoUrl,
                    v.VideoImage,
                    TotalLikes = _context.MobileVideoStats
                        .Where(s => s.VideoId == v.VideoId)
                        .Sum(s => s.LikeCount),

                    TotalUnlikes = _context.MobileVideoStats
                        .Where(s => s.VideoId == v.VideoId)
                        .Sum(s => s.UnlikeCount)
                })
                .ToListAsync();

            return Ok(videos);
        }
        [HttpGet("video-most-liked")]
        public async Task<IActionResult> GetMostLikedVideos([FromQuery] int top = 10)
        {
            if (top <= 0)
                top = 10;

            var videos = await _context.MobileVideoStats
                .GroupBy(s => s.VideoId)
                .Select(g => new
                {
                    VideoId = g.Key,
                    TotalLikes = g.Sum(x => x.LikeCount),
                    TotalUnlikes = g.Sum(x => x.UnlikeCount),
                    TotalViews = _context.VideoTotalView
                        .Where(v => v.VideoId == g.Key)
                        .Select(v => v.TotalViews)
                        .FirstOrDefault(),
                    EmbedCode = _context.VideoTotalView
                        .Where(v => v.VideoId == g.Key)
                        .Select(v => v.EmbedCode)
                        .FirstOrDefault(),
                    VideoUrl = _context.VideoTotalView
                        .Where(v => v.VideoId == g.Key)
                        .Select(v => v.VideoUrl)
                        .FirstOrDefault(),
                    VideoImage = _context.VideoTotalView
                        .Where(v => v.VideoId == g.Key)
                        .Select(v => v.VideoImage)
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.TotalLikes)
                .Take(top)
                .ToListAsync();

            return Ok(videos);
        }

        [HttpPost("video-stats-by-ids")]
        public async Task<IActionResult> GetVideoStatsByIds([FromBody] List<string> videoIds)
        {
            if (videoIds == null || videoIds.Count == 0)
                return BadRequest(new { message = "VideoId listesi boş olamaz." });

            // Toplu VideoTotalView kayıtlarını çekelim
            var totalViews = await _context.VideoTotalView
                .Where(v => videoIds.Contains(v.VideoId))
                .ToListAsync();

            // Like/Unlike toplamlarını gruplayalım
            var stats = await _context.MobileVideoStats
                .Where(s => videoIds.Contains(s.VideoId))
                .GroupBy(s => s.VideoId)
                .Select(g => new
                {
                    VideoId = g.Key,
                    TotalLikes = g.Sum(x => x.LikeCount),
                    TotalUnlikes = g.Sum(x => x.UnlikeCount)
                })
                .ToListAsync();

            // Birleştirip final listeyi oluşturalım
            var result = videoIds.Select(id =>
            {
                var view = totalViews.FirstOrDefault(v => v.VideoId == id);
                var stat = stats.FirstOrDefault(s => s.VideoId == id);

                return new
                {
                    VideoId = id,
                    TotalViews = view?.TotalViews ?? 0,
                    TotalLikes = stat?.TotalLikes ?? 0,
                    TotalUnlikes = stat?.TotalUnlikes ?? 0,
                    EmbedCode = view?.EmbedCode,
                    VideoUrl = view?.VideoUrl,
                    VideoImage = view?.VideoImage
                };
            });

            return Ok(result);
        }

        #region TeamSquadImage Endpoints

        /// <summary>
        /// Takım kadro görsellerini listeler
        /// </summary>
        [HttpGet("team-squad-images")]
        public async Task<IActionResult> GetTeamSquadImages([FromQuery] int? teamId = null)
        {
            var query = _context.TeamSquadImages.AsNoTracking();

            if (teamId.HasValue)
                query = query.Where(x => x.TeamId == teamId.Value);

            var images = await query
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.TeamId,
                    x.ImageUrl,
                    x.CreatedAt
                })
                .ToListAsync();

            return Ok(images);
        }

        /// <summary>
        /// Tek bir takım kadro görselini getirir
        /// </summary>
        [HttpGet("team-squad-images/{id}")]
        public async Task<IActionResult> GetTeamSquadImage(int id)
        {
            var image = await _context.TeamSquadImages
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.Id,
                    x.TeamId,
                    x.ImageUrl,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (image == null)
                return NotFound(new { message = "Görsel bulunamadı." });

            return Ok(image);
        }

        /// <summary>
        /// Yeni takım kadro görseli yükler (aynı TeamId varsa üzerine yazar)
        /// </summary>
        [HttpPost("team-squad-images")]
        public async Task<IActionResult> UploadTeamSquadImage([FromForm] TeamSquadImageUploadDto dto)
        {
            if (dto.Image == null || dto.Image.Length == 0)
                return BadRequest(new { message = "Görsel dosyası gereklidir." });

            if (dto.TeamId <= 0)
                return BadRequest(new { message = "Geçerli bir TeamId gereklidir." });

            // Dosya uzantısını kontrol et
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var extension = Path.GetExtension(dto.Image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Geçersiz dosya formatı. İzin verilen formatlar: jpg, jpeg, png, webp, gif" });

            try
            {
                // Bu takımın mevcut görseli var mı?
                var existingImage = await _context.TeamSquadImages
                    .FirstOrDefaultAsync(x => x.TeamId == dto.TeamId);

                if (existingImage != null)
                {
                    // Eski görseli R2'den sil
                    if (!string.IsNullOrWhiteSpace(existingImage.ImageKey))
                    {
                        try
                        {
                            await _r2Manager.DeleteFileAsync(existingImage.ImageKey);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Eski görsel silinirken hata oluştu: {ImageKey}", existingImage.ImageKey);
                        }
                    }

                    // Yeni görseli yükle
                    var fileName = $"squad-images/{dto.TeamId}/{Guid.NewGuid()}{extension}";
                    using var stream = dto.Image.OpenReadStream();
                    await _r2Manager.UploadFileAsync(fileName, stream, dto.Image.ContentType);

                    // Mevcut kaydı güncelle
                    existingImage.ImageKey = fileName;
                    existingImage.ImageUrl = _r2Manager.GetFileUrl(fileName);
                    existingImage.CreatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        existingImage.Id,
                        existingImage.TeamId,
                        existingImage.ImageUrl,
                        message = "Görsel başarıyla güncellendi."
                    });
                }
                else
                {
                    // Yeni görsel oluştur
                    var fileName = $"squad-images/{dto.TeamId}/{Guid.NewGuid()}{extension}";
                    using var stream = dto.Image.OpenReadStream();
                    await _r2Manager.UploadFileAsync(fileName, stream, dto.Image.ContentType);

                    var imageUrl = _r2Manager.GetFileUrl(fileName);

                    var squadImage = new TeamSquadImage
                    {
                        TeamId = dto.TeamId,
                        ImageKey = fileName,
                        ImageUrl = imageUrl,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.TeamSquadImages.Add(squadImage);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        squadImage.Id,
                        squadImage.TeamId,
                        squadImage.ImageUrl,
                        message = "Görsel başarıyla yüklendi."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Takım kadro görseli yüklenirken hata oluştu.");
                return StatusCode(500, new { message = "Görsel yüklenirken bir hata oluştu." });
            }
        }

        /// <summary>
        /// Takım kadro görselini siler
        /// </summary>
        [HttpDelete("team-squad-images/{id}")]
        public async Task<IActionResult> DeleteTeamSquadImage(int id)
        {
            var image = await _context.TeamSquadImages.FindAsync(id);
            if (image == null)
                return NotFound(new { message = "Görsel bulunamadı." });

            // R2'den görseli sil
            if (!string.IsNullOrWhiteSpace(image.ImageKey))
            {
                try
                {
                    await _r2Manager.DeleteFileAsync(image.ImageKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Görsel R2'den silinirken hata oluştu.");
                }
            }

            _context.TeamSquadImages.Remove(image);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Görsel başarıyla silindi." });
        }

        /// <summary>
        /// Bir takımın kadro görselini getirir
        /// </summary>
        [HttpGet("team-squad-images/by-team/{teamId}")]
        public async Task<IActionResult> GetTeamSquadImageByTeam(int teamId)
        {
            var image = await _context.TeamSquadImages
                .AsNoTracking()
                .Where(x => x.TeamId == teamId)
                .Select(x => new
                {
                    x.Id,
                    x.TeamId,
                    x.ImageUrl,
                    x.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (image == null)
                return NotFound(new { message = "Bu takıma ait görsel bulunamadı." });

            return Ok(image);
        }

        #endregion

    }
}