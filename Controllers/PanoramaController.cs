using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Rakipbul.Models;
using RakipBul.Data;
using System.Collections.Generic; 
using System.Linq;
using System.Threading.Tasks;

namespace Controllers
{
    public class PanoramaController : Controller
    {
        private readonly RakipbulApiManager _rakipbulApiManager;
        private readonly ApplicationDbContext _context;


        public PanoramaController(RakipbulApiManager rakipbulApiManager, ApplicationDbContext context)
        {
            _rakipbulApiManager = rakipbulApiManager;
            _context = context;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var leagues = await _rakipbulApiManager.GetLeaguesAsync();
            var model = new PanoramaViewModel
            {
                Leagues = leagues
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(PanoramaFormModel form)
        {
            var leagues = await _rakipbulApiManager.GetLeaguesAsync();
            List<RakipbulSeasonDto> seasons = new();

            // Sezonları tekrar doldur
            if (form.LeagueId.HasValue && form.LeagueId.Value > 0)
            {
                seasons = await _rakipbulApiManager.GetLeagueSeasonsAsync(form.LeagueId.Value);
            }

            // Aranan oyuncunun full datasını getirelim
            RakipbulPlayerDto selectedPlayer = null;

            if (!string.IsNullOrWhiteSpace(form.PlayerJson))
            {
                selectedPlayer = JsonConvert.DeserializeObject<RakipbulPlayerDto>(form.PlayerJson);
            }
            if (form.PlayerId.HasValue)
            {
                var searchResult = await _rakipbulApiManager.SearchAsync(form.PlayerName ?? "");
                selectedPlayer = searchResult.Player.FirstOrDefault(p => p.Id == form.PlayerId.Value);
            }

            // ✔ Kayıt oluştur
            var entry = new PanoramaEntry
            {
                Category = form.ActiveTab == 1 ? PanoramaCategory.Panorama : PanoramaCategory.Goals,
                Title = form.Title,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
                YoutubeEmbedLink = form.YoutubeEmbedLink,

                // Player
                PlayerId = form.PlayerId,
                PlayerName = form.PlayerName,
                PlayerImageUrl = selectedPlayer?.Image_Url,
                PlayerPosition = selectedPlayer?.Position?.Name ?? "",

                // Team
                TeamId = selectedPlayer?.Team?.Id,
                TeamName = selectedPlayer?.Team?.Name,
                TeamImageUrl = selectedPlayer?.Team?.Image_Url,
                SeasonId=form.SeasonId,
                // League + Province
                LeagueId = form.LeagueId,
                LeagueName = selectedPlayer?.Team?.League?.Name,
                ProvinceName = selectedPlayer?.Team?.League?.Province?.Name
            };

            _context.PanoramaEntries.Add(entry);

            await _context.SaveChangesAsync();

            TempData["Success"] = form.ActiveTab == 1
                ? "Panorama kaydedildi."
                : "Haftanın golleri kaydedildi.";

            // Modeli geri doldur
            var model = new PanoramaViewModel
            {
                Leagues = leagues,
                Seasons = seasons,
                SelectedLeagueId = form.LeagueId,
                SelectedSeason = form.Season,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
                YoutubeEmbedLink = form.YoutubeEmbedLink,
                ActiveTab = form.ActiveTab,
                Title = form.Title,
                PlayerId = form.PlayerId,
                PlayerName = form.PlayerName
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> GetSeasons(int leagueId)
        {
            var seasons = await _rakipbulApiManager.GetLeagueSeasonsAsync(leagueId);
            return Json(seasons);
        }

        [HttpGet]
        public async Task<IActionResult> SearchPlayers(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
                return Json(new List<RakipbulPlayerDto>());

            var result = await _rakipbulApiManager.SearchAsync(term);
            return Json(result.Player); // SearchResponse içinde Players olduğunu varsayıyoruz
        }


    }
}
