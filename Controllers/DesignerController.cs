using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Controllers
{
    public class DesignerController : Controller
    {
        private readonly RakipbulApiManager _rakipbulApiManager;

        public DesignerController(RakipbulApiManager rakipbulApiManager)
        {
            _rakipbulApiManager = rakipbulApiManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var leagues = await _rakipbulApiManager.GetLeaguesAsync();
            ViewBag.Leagues = leagues;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSeasons(int leagueId)
        {
            var seasons = await _rakipbulApiManager.GetLeagueSeasonsAsync(leagueId);
            return Json(seasons);
        }

        [HttpGet]
        public async Task<IActionResult> GetLeagues()
        {
            var leagues = await _rakipbulApiManager.GetLeaguesAsync();
            return Json(leagues);
        }

        [HttpGet]
        public async Task<IActionResult> GetMatches(int leagueId, int seasonId, string date)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
            {
                return BadRequest("Geçersiz tarih formatı");
            }
            var matches = await _rakipbulApiManager.GetMatchesByDateAsync(leagueId, seasonId, parsedDate);
            return Json(matches);
        }
    }
}
