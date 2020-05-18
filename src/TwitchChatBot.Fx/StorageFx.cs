﻿using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Enums;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.Fx
{
    [StorageAccount("Storage")]
    public class StorageFx
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStorageService _storageService;
        public StorageFx(IConfiguration configuration, IHttpClientFactory httpClientFactory, IStorageService storageService)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _storageService = storageService;
        }

        /* [FunctionName("ProcessQueueEntry")]
         public async Task ProcessQueueEntry(
             [QueueTrigger(Constants.QUEUE_NAME, Connection = Constants.QUEUE_STORAGE_CONNECTION)] ChannelActivityEntity myQueueItem, ILogger logger)
         {
             logger.LogInformation($"{DateTime.UtcNow}: Starting the function");
             var formData = new Dictionary<string, string>
             {
                 {"viewer",myQueueItem.Viewer },
                 { "activity",myQueueItem.Activity },
                 {"channel", myQueueItem.PartitionKey },
                 {"timestamp", myQueueItem.RowKey }
             };

             var data = new FormUrlEncodedContent(formData);
             var client = _httpClientFactory.CreateClient(Constants.FX_HTTPCLIENT_NAME);
             try
             {
                 logger.LogInformation($"{DateTime.UtcNow}: Sending the data for {myQueueItem.PartitionKey}-{myQueueItem.Activity} to Zapier");
                 //TODO: Setup Zapier to read from config settings
                 var response = await client.PostAsync("hooks/catch/3191324/o5k3wf7", data);
                 response.EnsureSuccessStatusCode();
                 logger.LogInformation($"{DateTime.UtcNow}: Sent the data for {myQueueItem.PartitionKey}-{myQueueItem.Activity} to Zapier");
             }
             catch (Exception ex)
             {
                 logger.LogError($"{DateTime.UtcNow}: Ran into an error sending the data for {myQueueItem.PartitionKey}-{myQueueItem.Activity} to Zapier", ex);
             }
             logger.LogInformation($"{DateTime.UtcNow}: Completed the function");

         }*/

        [FunctionName("ProcessFollowersQueueEntry")]
        public async Task ProcessFollowersQueueMessage([QueueTrigger(Constants.CONFIG_FOLLOWERS_QUEUE_NAME_VALUE, Connection = Constants.CONFIG_CONNSTRING_STORAGE_NAME)] string message, ILogger logger)
        {
            var json = JObject.Parse(message);
            var updates = json["data"].ToObject<List<TwitchWebhookFollowersResponse>>();
            foreach (var update in updates)
            {
                var entity = new ChannelActivityEntity
                {
                    Activity = StreamActivity.UserFollowed.ToString(),
                    PartitionKey = update.ToName,
                    RowKey = update.FollowedAt.ToString("s").Replace(":", string.Empty).Replace("-", string.Empty),
                    Viewer = update.FromName
                };

                await _storageService.AddDataToStorage(entity);
            }
        }

        [FunctionName("ProcessStreamQueueEntry")]
        public async Task ProcessStreamQueueMessage([QueueTrigger(Constants.CONFIG_STREAM_QUEUE_NAME_VALUE, Connection = Constants.CONFIG_CONNSTRING_STORAGE_NAME)] string message, ILogger logger)
        {
            var json = JObject.Parse(message);
            var updates = json["data"].ToObject<List<TwitchWebhookStreamResponse>>();
            foreach (var update in updates)
            {
                var entity = new ChannelActivityEntity
                {
                    Activity = StreamActivity.StreamStarted.ToString(),
                    PartitionKey = update.UserName,
                    RowKey = update.StartedAt.ToString("s").Replace(":", string.Empty).Replace("-", string.Empty),
                    Viewer = string.Empty
                };

                await _storageService.AddDataToStorage(entity);
            }
        }
    }
}