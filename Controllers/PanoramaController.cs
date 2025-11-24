using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic; 

namespace Controllers
{
    public class PanoramaController : Controller
    {
        private readonly RakipbulApiManager _rakipbulApiManager;

        public PanoramaController(RakipbulApiManager rakipbulApiManager)
        {
            _rakipbulApiManager = rakipbulApiManager;
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

            if (form.LeagueId.HasValue && form.LeagueId.Value > 0)
            {
                seasons = await _rakipbulApiManager.GetLeagueSeasonsAsync(form.LeagueId.Value);
            }

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

            // ActiveTab == 1 => Panorama kaydet
            // ActiveTab == 2 => Haftanın Golleri kaydet
            // Burada sen ayrı kayıt logic’ini yazarsın.

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
