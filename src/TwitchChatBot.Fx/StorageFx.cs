using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Enums;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.Fx
{
    public class StorageFx
    {
        private readonly IConfiguration _configuration;
        private readonly IStorageService _storageService;
        private readonly HubConnection _hubConnection;
        public StorageFx(IConfiguration configuration, IStorageService storageService)
        {
            _configuration = configuration;
            _storageService = storageService;

            var connectionString = _configuration[Constants.FX_CONFIG_CONNSTRING_STORAGE_NAME];
            _storageService.SetConnectionString(connectionString);

            var signalRUri = "https://twitchchatbotwebv220200523131946.azurewebsites.net/";
            var signalRHub = "twitchhub";

            var uri = new Uri($"{signalRUri}/{signalRHub}");
            _hubConnection = new HubConnectionBuilder()
                .WithAutomaticReconnect()
                .WithUrl(uri)
                .Build();
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
        public async Task ProcessFollowersQueueMessage(
            [QueueTrigger("followers-data")] string message, ILogger logger)
        {
            var json = JObject.Parse(message);
            var updates = json["data"].ToObject<List<TwitchWebhookFollowersResponse>>();
            foreach (var update in updates)
            {
                var entity = new ChannelActivityEntity
                {
                    Activity = StreamActivity.UserFollowed.ToString(),
                    PartitionKey = update.ToName,
                    RowKey = update.FollowedAt.ToString(Constants.DATETIME_FORMAT).Replace(":", string.Empty).Replace("-", string.Empty),
                    Viewer = update.FromName
                };

                await _storageService.AddDataToStorage(entity);
            }
        }

        [FunctionName("ProcessStreamQueueEntry")]
        public async Task ProcessStreamQueueMessage(
            [QueueTrigger(Constants.FX_CONFIG_STREAM_QUEUE_NAME_VALUE)] string message, ILogger logger)
        {
            var json = JObject.Parse(message);
            var entity = new ChannelActivityEntity();
            if (json["data"].HasValues)
            {
                var updates = json["data"].ToObject<List<TwitchWebhookStreamResponse>>();
                foreach (var update in updates)
                {
                    entity.Activity = StreamActivity.StreamStarted.ToString();
                    entity.PartitionKey = update.UserName;
                    entity.RowKey = update.StartedAt.ToString(Constants.DATETIME_FORMAT).Replace(":", string.Empty).Replace("-", string.Empty);
                    entity.Viewer = string.Empty;
                }
            }
            else
            {
                entity.Activity = StreamActivity.StreamStopped.ToString();
                entity.PartitionKey = json["channel"].ToString();
                entity.RowKey = DateTime.Parse(json["timestamp"].ToString()).ToString(Constants.DATETIME_FORMAT).Replace(":", string.Empty).Replace("-", string.Empty);
                entity.Viewer = string.Empty;
            }
            await _storageService.AddDataToStorage(entity);
            await DispatchSignalRMessage(entity);
        }

        private async Task DispatchSignalRMessage(ChannelActivityEntity entity)
        {
            try
            {
                await _hubConnection.StartAsync();
                await _hubConnection.InvokeAsync("StreamUpdate", entity);
            }
            finally
            {
                await _hubConnection.StopAsync();
            }
        }
    }
}