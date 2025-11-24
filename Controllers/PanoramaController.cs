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
            if (form.LeagueId > 0)
            {
                seasons = await _rakipbulApiManager.GetLeagueSeasonsAsync(form.LeagueId);
            }
            var model = new PanoramaViewModel
            {
                Leagues = leagues,
                Seasons = seasons,
                SelectedLeagueId = form.LeagueId,
                SelectedSeason = form.Season,
                StartDate = form.StartDate,
                EndDate = form.EndDate,
                YoutubeEmbedLink = form.YoutubeEmbedLink
            };
            return View(model);
        }
    }
}
