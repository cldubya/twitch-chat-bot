using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Enums;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.Web.v2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly ILogger<SubscriptionController> _logger;
        private readonly IStorageService _storageService;

        public SubscriptionController(ILogger<SubscriptionController> logger, IStorageService storageService)
        {
            _logger = logger;
            _storageService = storageService;
        }

        [AllowAnonymous]
        [HttpPost("followers/{channel}")]
        public async Task<IActionResult> PostFollowerUpdate([FromBody] object data, string channel)
        {
            var json = JObject.Parse(data.ToString());
            var updates = json["data"].ToObject<List<TwitchWebhookFollowersResponse>>();
            foreach (var update in updates)
            {
                var entity = new ChannelActivityEntity
                {
                    Activity = StreamActivity.UserFollowed.ToString(),
                    PartitionKey = channel,
                    RowKey = update.FollowedAt.ToString("s").Replace(":", string.Empty).Replace("-", string.Empty),
                    Viewer = update.FromName
                };

                await _storageService.AddDataToStorage(entity);
            }
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("followers/{channel}")]
        public async Task<IActionResult> GetFollowerUpdate([FromQuery] TwitchWebhookResponse response, string channel)
        {
            await Task.CompletedTask;
            return Content(response.Challenge, "text/plain");
        }

        [AllowAnonymous]
        [HttpPost("streams/{channel}")]
        public async Task<IActionResult> PostStreamUpdate([FromBody] object data, string channel)
        {
            await Task.CompletedTask;
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("streams/{channel}")]
        public async Task<IActionResult> GetStreamerUpdate([FromQuery] TwitchWebhookResponse response, string channel)
        {
            await Task.CompletedTask;
            return Content(response.Challenge, "text/plain");
        }
    }
}
