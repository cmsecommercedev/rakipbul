using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed; // IDistributedCache için (Opsiyonel ama iyi pratik)
using Microsoft.Extensions.Caching.Memory; // IMemoryCache için
using Microsoft.Extensions.Logging;
using Rakipbul.DTOs;
using Rakipbul.Models;
using RakipBul.Attributes; // Kullanıcının kimliğini almak için (opsiyonel)
using RakipBul.Data;
using RakipBul.Dtos;
using RakipBul.Managers;    // NotificationManager için
using RakipBul.Models;
using RakipBul.Models.Dtos; // Eklediğimiz DTO'lar için
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RakipBul.Controllers.Api // Namespace'i kontrol edin
{
    [ApiKeyAuth]
    [Route("api/[controller]")]
    [ApiController]
    public class FavouritesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FavouritesController> _logger;
        private readonly IMemoryCache _cache;
        private readonly NotificationManager _notificationManager;

        public FavouritesController(ApplicationDbContext context, ILogger<FavouritesController> logger, IMemoryCache cache, NotificationManager notificationManager)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
            _notificationManager = notificationManager;
        }


        [HttpPost("register-device")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest req)
        {
            try
            {
                if (req.UserId <= 0 || string.IsNullOrWhiteSpace(req.DeviceToken))
                    return BadRequest("Geçersiz kullanıcı veya device token."); 

                // Veri var mı?
                var existing = await _context.UserDeviceToken
                    .FirstOrDefaultAsync(x => x.UserId == req.UserId && x.Token == req.DeviceToken);

                if (existing != null)
                {
                    // Güncelle
                    existing.Culture = req.Culture;
                    existing.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return Ok(new { success = true, message = "Device token güncellendi." });
                }

                // Yeni kayıt oluştur
                var device = new UserDeviceToken
                {
                    UserId = req.UserId,
                    Token = req.DeviceToken,
                    Culture = req.Culture,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserDeviceToken.Add(device);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Device başarıyla kaydedildi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Device kaydedilirken hata oluştu.");
                return StatusCode(500, "Sunucu hatası");
            }
        } 

        [HttpPost("addteamtofav")]
        public async Task<IActionResult> AddTeamToFav(int teamId, string userToken, string MacID)
        {
            if (teamId <= 0 || string.IsNullOrWhiteSpace(userToken))
                return BadRequest("Geçersiz veri.");

            bool exists = await _context.FavouriteTeams
                .AnyAsync(f => f.TeamID == teamId && f.UserToken == userToken);

            if (exists)
                return Ok(new { success = true, message = "Zaten favorilerde." });

            _context.FavouriteTeams.Add(new FavouriteTeams
            {
                TeamID = teamId,
                UserToken = userToken,
                MacID = MacID
            });

            await _context.SaveChangesAsync();

            string topic = $"team_{teamId}";

            await _notificationManager.SubscribeToTopicAsync(userToken, topic);

            return Ok(new { success = true, message = "Favorilere eklendi." });
        }


        [HttpPost("removeteamfromfav")]
        public async Task<IActionResult> RemoveTeamFromFav([FromQuery] int teamId, [FromQuery] string userToken, string MacID)
        {
            if (teamId <= 0 || string.IsNullOrWhiteSpace(userToken))
                return BadRequest("Geçersiz takım veya kullanıcı token bilgisi.");

            // Favori kaydını bul
            var fav = await _context.FavouriteTeams
                .FirstOrDefaultAsync(f => f.TeamID == teamId && f.MacID == MacID);

            if (fav == null)
                return Ok(new { success = true, message = "Bu takım zaten favorilerde değil." });

            // Tablo kaydını sil
            _context.FavouriteTeams.Remove(fav);

            await _context.SaveChangesAsync();

            // Firebase topic'ten çıkar - kaydedilen culture'ı kullan

            string topic = $"team_{teamId}";

            var result = await _notificationManager.UnsubscribeFromTopicAsync(userToken, topic);

            if (result.success)
                return Ok(new { success = true, message = $"Takım favorilerden çıkarıldı ve topic'ten çıkıldı: {topic}" });
            else
                return StatusCode(500, new { success = false, message = $"Favorilerden çıkarıldı fakat topic'ten çıkılamadı: {result.message}" });
        }

        [HttpPost("addplayertofav")]
        public async Task<IActionResult> AddPlayerToFav([FromQuery] int playerId, [FromQuery] string userToken, string MacID)
        {
            // Zaten favori mi kontrol et
            bool alreadyExists = await _context.FavouritePlayers
                .AnyAsync(f => f.PlayerID == playerId && f.UserToken == userToken);

            if (alreadyExists)
                return Ok(new { success = true, message = "Bu oyuncu zaten favorilerde." });

            // Tabloya ekle
            var fav = new FavouritePlayers
            {
                PlayerID = playerId,
                UserToken = userToken,
                MacID = MacID
            };
            _context.FavouritePlayers.Add(fav);
            await _context.SaveChangesAsync();

            // Firebase topic abone et
            string topic = $"player_{playerId}";

            var result = await _notificationManager.SubscribeToTopicAsync(userToken, topic);

            if (result.success)
                return Ok(new { success = true, message = $"Oyuncu favorilere eklendi ve topic'e abone olundu: {topic}" });
            else
                return StatusCode(500, new { success = false, message = $"Favorilere eklendi fakat topic abonesi yapılamadı: {result.message}" });
        }

        [HttpPost("removeplayerfromfav")]
        public async Task<IActionResult> RemovePlayerFromFav([FromQuery] int playerId, [FromQuery] string userToken, string MacID)
        {
            if (playerId <= 0 || string.IsNullOrWhiteSpace(userToken))
                return BadRequest("Geçersiz oyuncu veya kullanıcı token bilgisi.");

            // Favori kaydını bul
            var fav = await _context.FavouritePlayers
                .FirstOrDefaultAsync(f => f.PlayerID == playerId && f.MacID == MacID);

            if (fav == null)
                return Ok(new { success = true, message = "Bu oyuncu zaten favorilerde değil." });

            // Tablo kaydını sil
            _context.FavouritePlayers.Remove(fav);

            await _context.SaveChangesAsync();

            string topic = $"player_{playerId}";

            var result = await _notificationManager.UnsubscribeFromTopicAsync(userToken, topic);

            if (result.success)
                return Ok(new { success = true, message = $"Oyuncu favorilerden çıkarıldı ve topic'ten çıkıldı: {topic}" });
            else
                return StatusCode(500, new { success = false, message = $"Favorilerden çıkarıldı fakat topic'ten çıkılamadı: {result.message}" });
        }

        [HttpGet("isplayerfav")]
        public async Task<IActionResult> IsPlayerFav([FromQuery] int playerId, [FromQuery] string macId)
        {
            if (playerId <= 0 || string.IsNullOrWhiteSpace(macId))
                return BadRequest("Geçersiz oyuncu veya cihaz bilgisi.");

            bool isFav = await _context.FavouritePlayers.AnyAsync(f => f.PlayerID == playerId && f.MacID == macId);

            return Ok(new { isFavourite = isFav });
        }

        [HttpGet("isteamfav")]
        public async Task<IActionResult> IsTeamFav([FromQuery] int teamId, [FromQuery] string macId)
        {
            if (teamId <= 0 || string.IsNullOrWhiteSpace(macId))
                return BadRequest("Geçersiz takım veya cihaz bilgisi.");

            bool isFav = await _context.FavouriteTeams.AnyAsync(f => f.TeamID == teamId && f.MacID == macId);

            return Ok(new { isFavourite = isFav });
        }

    }
}