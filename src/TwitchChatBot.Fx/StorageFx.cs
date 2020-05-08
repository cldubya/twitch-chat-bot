using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.Fx
{
    [StorageAccount("Storage")]
    public class StorageFx
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public StorageFx(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [FunctionName("ProcessQueueEntry")]
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

        }
    }
}