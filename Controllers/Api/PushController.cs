using RakipBul.Attributes;
using RakipBul.Managers;
using RakipBul.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RakipBul.Controllers.Api
{
    [ApiKeyAuth]
    [Route("api/[controller]")]
    [ApiController]
    public class PushController : ControllerBase
    {
        private readonly NotificationManager _notificationManager;
        private readonly ILogger<PushController> _logger;

        public PushController(NotificationManager notificationManager, ILogger<PushController> logger)
        {
            _notificationManager = notificationManager;
            _logger = logger;
        }

        [HttpPost("send-everyone")]
        public async Task<IActionResult> Send([FromBody] NotificationViewModel model,string culture)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var topic = $"all_users_{culture}";

            var result = await _notificationManager.SendNotificationToAllUsers(model,topic);
            if (result.success)
                return Ok(new { success = true, message = result.message });

            _logger.LogWarning("Push send failed: {Message}", result.message);
            return StatusCode(500, new { success = false, message = result.message });
        }

        [HttpPost("send-team")]
        public async Task<IActionResult> SendTeam([FromBody] NotificationViewModel model,int teamid, string culture)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string topic = $"team_{teamid}_{culture}";


            var result = await _notificationManager.SendNotificationToAllUsers(model, topic);
            if (result.success)
                return Ok(new { success = true, message = result.message });

            _logger.LogWarning("Push send failed: {Message}", result.message);
            return StatusCode(500, new { success = false, message = result.message });
        }

        [HttpPost("send-player")]
        public async Task<IActionResult> SendPlayer([FromBody] NotificationViewModel model, int playerid, string culture)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string topic = $"player_{playerid}_{culture}";

            var result = await _notificationManager.SendNotificationToAllUsers(model, topic);
            if (result.success)
                return Ok(new { success = true, message = result.message });

            _logger.LogWarning("Push send failed: {Message}", result.message);
            return StatusCode(500, new { success = false, message = result.message });
        }
    }
}


