using Azure.Storage.Queues;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Enums;
using TwitchChatBot.Shared.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchChatBot.CLI
{

    public class Bot : IDisposable
    {
        private TwitchClient _client;
        //private readonly CloudTable _tableClient;
        private QueueClient _queueClient;
        private readonly IConfiguration _config;

        public Bot(IConfiguration config)
        {
            _config = config;
        }

        public async Task Initialize()
        {
            await SetupStorage();
            await SetupTwitchClient();
        }

        private async Task SetupStorage()
        {
            /* Common.CreateTableStorageAccount(_config.GetConnectionString("Storage"));
             _tableClient = await Common.CreateTableAsync();*/

            _queueClient = await Common.CreateQueue(Constants.CONFIG_QUEUE_NAME_VALUE);
        }

        private async Task SetupTwitchClient()
        {
            // TwitchApps TMI - https://twitchapps.com/tmi/
            var credentials =
                new ConnectionCredentials(_config[Constants.CONFIG_TWITCH_USERNAME], _config[Constants.CONFIG_TWITCH_PASSWORD]);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            var customClient = new WebSocketClient(clientOptions);
            _client = new TwitchClient(customClient);
            _client.Initialize(credentials);
            _client.AutoReListenOnException = true;

            _client.OnLog += Client_OnLog;
            _client.OnJoinedChannel += Client_OnJoinedChannel;
            _client.OnConnected += Client_OnConnected;
            _client.OnMessageReceived += async (s, e) => await Client_OnMessageReceived(s, e);
            _client.OnNewSubscriber += async (s, e) => await Client_OnNewSubscriber(s, e);
            _client.OnUserJoined += async (s, e) => await Client_OnUserJoined(s, e);
            _client.OnUserLeft += async (s, e) => await Client_OnUserLeft(s, e);

            _client.Connect();

            await Task.CompletedTask;
        }

        public async Task Start(string channel)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Joining the channel {channel}");
            if (!_client.JoinedChannels.Any(x =>
                string.Equals(channel, x.Channel, StringComparison.InvariantCultureIgnoreCase) && _client.IsConnected))
            {
                _client.JoinChannel(channel);
            }
            var entity = new ChannelActivityEntity
            {
                PartitionKey = channel,
                RowKey = DateTime.UtcNow.ToString("s").Replace(":", string.Empty).Replace("-", string.Empty),
                Activity = StreamActivity.StreamStarted.ToString(),
                Viewer = ""
            };
            await AddEntityToStorage(entity);
            Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Joined the channel {channel}");

            await Task.CompletedTask;
        }

        public async Task Stop(string channel)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Stopping the channel {channel}");
            if (_client.JoinedChannels.Any(x =>
                string.Equals(x.Channel, channel, StringComparison.InvariantCultureIgnoreCase)))
            {
                Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Leaving the channel {channel}");
                _client.LeaveChannel(channel);
                Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Left the channel {channel}");
            }
            var entity = new ChannelActivityEntity
            {
                PartitionKey = channel,
                RowKey = DateTime.UtcNow.ToString("s").Replace(":", string.Empty).Replace("-", string.Empty),
                Activity = StreamActivity.StreamStopped.ToString(),
                Viewer = ""
            };
            await AddEntityToStorage(entity);
            Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Stopped the channel {channel}");

            await Task.CompletedTask;
        }

        public async Task AddNewFollowers(string channel, List<string> followers)
        {
            foreach (var follower in followers)
            {
                var entity = new ChannelActivityEntity
                {
                    PartitionKey = channel,
                    RowKey = DateTime.UtcNow.ToString("s").Replace(":", string.Empty).Replace("-", string.Empty),
                    Activity = StreamActivity.UserFollowed.ToString(),
                    Viewer = follower
                };
                await AddEntityToStorage(entity);
            }
        }

        private async Task Client_OnUserJoined(object sender, OnUserJoinedArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: {e.Username} joined the channel ({e.Channel})");

            var entity = new ChannelActivityEntity
            {
                PartitionKey = e.Channel,
                RowKey = DateTime.UtcNow.ToString("s").Replace(":", string.Empty).Replace("-", string.Empty),
                Activity = StreamActivity.UserJoined.ToString(),
                Viewer = e.Username
            };
            await AddEntityToStorage(entity);
        }

        private void Client_OnLog(object sender, OnLogArgs e) => Console.WriteLine($"{e.DateTime.ToUniversalTime().ToString(CultureInfo.InvariantCulture)}: {e.BotUsername} - {e.Data}");

        private void Client_OnConnected(object sender, OnConnectedArgs e) => Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Connected to Twitch");

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e) => Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Completed joining the channel {e.Channel}");

        private async Task AddEntityToStorage(ChannelActivityEntity entity)
        {
            await Common.AddMessageToQueue(_queueClient, entity);
            //await Common.InsertOrMergeEntityAsync(_tableClient, entity);
        }

        private async Task Client_OnUserLeft(object sender, OnUserLeftArgs e)
        {
            Console.WriteLine(
                $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: {e.Username} left the channel {e.Channel}");
            var entity = new ChannelActivityEntity
            {
                PartitionKey = e.Channel,
                RowKey = DateTime.UtcNow.ToString("s").Replace(":", string.Empty).Replace("-", string.Empty),
                Activity = StreamActivity.UserLeft.ToString(),
                Viewer = e.Username
            };

            await AddEntityToStorage(entity);
        }

        private async Task Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Message Posted");
            var entity = new ChannelActivityEntity
            {
                PartitionKey = e.ChatMessage.Channel,
                RowKey = DateTime.UtcNow.ToString("s").Replace(":", string.Empty).Replace("-", string.Empty),
                Activity = StreamActivity.MessagePosted.ToString(),
                Viewer = e.ChatMessage.Username
            };
            await AddEntityToStorage(entity);
        }
        private async Task Client_OnNewSubscriber(object sender, OnNewSubscriberArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: New Subscriber Posted");
            var entity = new ChannelActivityEntity
            {
                PartitionKey = e.Channel,
                RowKey = DateTime.UtcNow.ToString("s").Replace(":", string.Empty).Replace("-", string.Empty),
                Activity = StreamActivity.UserSubscribed.ToString(),
                Viewer = e.Subscriber.Id
            };
            await AddEntityToStorage(entity);
        }

        public void Dispose()
        {
            if (!_client.JoinedChannels.Any()) return;
            var tasks = new List<Task>(_client.JoinedChannels.Count);
            tasks.AddRange(_client.JoinedChannels.Select(channel => Stop(channel.Channel)));

            Task.WaitAll(tasks.ToArray());
            _client.Disconnect();
        }
    }
}