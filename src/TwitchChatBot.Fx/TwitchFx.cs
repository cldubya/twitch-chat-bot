using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Models;
using TwitchLib.Api.Core.Models.Undocumented.Comments;

namespace TwitchChatBot.Fx
{
    public class TwitchFx
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        internal readonly CloudStorageAccount _storageAccount;
        internal CloudQueueClient _cloudQueueClient;
        internal CloudQueue _cloudQueue;
        public TwitchFx(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _storageAccount = CloudStorageAccount.Parse(_configuration[Constants.FX_CONFIG_CONNSTRING_STORAGE_NAME]);
            _cloudQueueClient = _storageAccount.CreateCloudQueueClient();
        }

        [FunctionName("ConfirmTwitchFollowersSubscription")]
        public async Task<IActionResult> ConfirmFollowersSubscription([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subscription/followers/{channel}")] HttpRequest request, string channel, ILogger logger)
        {
            logger.LogInformation($"{DateTime.UtcNow}: Confirming Follower Subscription for {channel}");
            var result = await ConfirmChallengeRequest(request.Query, logger);
            logger.LogInformation($"{DateTime.UtcNow}: Confirmed Follower Subscription for {channel}");
            return result;
        }

        [FunctionName("ProcessTwitchFollowersWebhook")]
        public async Task<IActionResult> HandleFollowersWebhookEvent([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscription/followers/{channel}")] HttpRequestMessage request, string channel, ILogger logger)
        {
            logger.LogInformation($"{DateTime.UtcNow}: Processing Twitch webhook for event on channel: {channel}");
            var messageText = await request.Content.ReadAsStringAsync();
            _cloudQueue = _cloudQueueClient.GetQueueReference(Constants.FX_CONFIG_FOLLOWERS_QUEUE_NAME_VALUE);
            var message = new CloudQueueMessage(messageText);
            await _cloudQueue.AddMessageAsync(message);
            return new NoContentResult();
        }

        [FunctionName("ConfirmTwitchStreamSubscription")]
        public async Task<IActionResult> ConfirmFollowSubscription([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "subscription/streams/{channel}")] HttpRequest request, string channel, ILogger logger)
        {
            logger.LogInformation($"{DateTime.UtcNow}: Confirming Stream Event Subscription for {channel}");
            var result = await ConfirmChallengeRequest(request.Query, logger);
            logger.LogInformation($"{DateTime.UtcNow}: Confirmed Stream Event Subscription for {channel}");
            return result;
        }

        [FunctionName("ProcessTwitchStreamWebhook")]
        public async Task<IActionResult> HandleStreamWebhookEvent([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "subscription/streams/{channel}")] HttpRequestMessage request, string channel, ILogger logger)
         {
            logger.LogInformation($"{DateTime.UtcNow}: Processing Twitch webhook for event on channel: {channel}");
            var messageText = await request.Content.ReadAsStringAsync();

            // Check if the json["data"] has values. If not add to the json children the current channel and the timestamp
            var json = JObject.Parse(messageText);

            if (!json["data"].HasValues)
            {
                json.Add("channel", channel);
                var timeStamp = request.Headers.FirstOrDefault(x => string.Equals(x.Key, "Twitch-Notification-Timestamp", StringComparison.InvariantCultureIgnoreCase)).Value.FirstOrDefault();
                json.Add("timestamp", timeStamp );
                messageText = json.ToString();
            }

            _cloudQueue = _cloudQueueClient.GetQueueReference(Constants.FX_CONFIG_STREAM_QUEUE_NAME_VALUE);
            var message = new CloudQueueMessage(messageText);
            await _cloudQueue.AddMessageAsync(message);
            return await Task.FromResult(new NoContentResult());
        }



        private async Task<IActionResult> ConfirmChallengeRequest(IQueryCollection keyValuePairs, ILogger logger)
        {
            const string CHALLENGE_KEY = "hub.challenge";
            if (!keyValuePairs.ContainsKey(CHALLENGE_KEY))
            {
                var errorMessage = $"{DateTime.UtcNow}: The hub.challenge parameter was not sent from Twitch";
                logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            StringValues challenge;
            var flag = keyValuePairs.TryGetValue(CHALLENGE_KEY, out challenge);
            logger.LogInformation($"{DateTime.UtcNow}: Confirming Challenge request with string {challenge}");
            return await Task.FromResult(new ContentResult { Content = challenge, ContentType = "text/plain", StatusCode = 200 });
        }
    }
}
