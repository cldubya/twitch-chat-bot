using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.Fx
{
    public class TwitchFx
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public TwitchFx(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        // Twitch Docs here: https://dev.twitch.tv/docs/api/webhooks-reference#subscribe-tounsubscribe-from-events
        [FunctionName("Subscribe_To_Timer")]
        public async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log)
        {
            if (myTimer.IsPastDue)
            {
                log.LogInformation("Timer is running late!");
            }
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");

            var client = _httpClientFactory.CreateClient(Constants.FX_TWITCH_WEBHOOKS_NAME);
            var formData = new Dictionary<string, string>
            {
                {"hub.callback",""},
                { "hub.mode","subscribe" },
                {"hub.topic", "" },
                {"hub.lease_seconds", Constants.TWITCH_WEBHOOKS_LEASE_MAX.ToString() }
            };

            var data = new FormUrlEncodedContent(formData);
            var response = await client.PostAsJsonAsync("helix/webhooks/hub", data);

        }


        [FunctionName("FollowSubscription")]
        public async Task HandleFollowSubscriptionUpdate([HttpTrigger(AuthorizationLevel.Anonymous,"get","post",Route = "subscription/follows/{channel}")] HttpRequest req, string channel, ILogger logger)
        {
           

            await Task.CompletedTask;
        }
    }
}
