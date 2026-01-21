using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Controllers
{
    public class DesignerController : Controller
    {
        private readonly RakipbulApiManager _rakipbulApiManager;
        private readonly ILogger<DesignerController> _logger;

        public DesignerController(RakipbulApiManager rakipbulApiManager, ILogger<DesignerController> logger)
        {
            _rakipbulApiManager = rakipbulApiManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var leagues = await _rakipbulApiManager.GetLeaguesAsync();
                ViewBag.Leagues = leagues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ligler yüklenirken hata oluştu");
                ViewBag.Leagues = new List<RakipbulLeagueDto>();
                ViewBag.Error = "Ligler yüklenemedi: " + ex.Message;
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSeasons(int leagueId)
        {
            try
            {
                var seasons = await _rakipbulApiManager.GetLeagueSeasonsAsync(leagueId);
                return Json(seasons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sezonlar yüklenirken hata oluştu. LeagueId: {LeagueId}", leagueId);
                return Json(new List<RakipbulSeasonDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLeagues()
        {
            try
            {
                var leagues = await _rakipbulApiManager.GetLeaguesAsync();
                return Json(leagues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ligler yüklenirken hata oluştu");
                return Json(new List<RakipbulLeagueDto>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMatches(int leagueId, int seasonId, string date)
        {
            try
            {
                if (!DateTime.TryParse(date, out var parsedDate))
                {
                    return BadRequest("Geçersiz tarih formatı");
                }
                var matches = await _rakipbulApiManager.GetMatchesByDateAsync(leagueId, seasonId, parsedDate);
                return Json(matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Maçlar yüklenirken hata oluştu. LeagueId: {LeagueId}, SeasonId: {SeasonId}, Date: {Date}", leagueId, seasonId, date);
                return Json(new List<RakipbulMatchDto>());
            }
        }

        /// <summary>
        /// External resimleri proxy üzerinden getirir (CORS sorunu çözmek için)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ImageProxy(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL gerekli");
            }

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return NotFound("Resim bulunamadı");
                }

                var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/png";
                var imageBytes = await response.Content.ReadAsByteArrayAsync();
                
                // Cache header ekle
                Response.Headers["Cache-Control"] = "public, max-age=86400"; // 1 gün cache
                
                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resim proxy hatası. URL: {Url}", url);
                return NotFound("Resim yüklenemedi");
            }
        }
    }
}
