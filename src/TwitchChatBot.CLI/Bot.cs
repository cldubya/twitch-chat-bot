using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Enums;
using TwitchChatBot.Shared.Models;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Extensions;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchChatBot.CLI
{
    public class Bot : IDisposable
    {
        private TwitchClient _client;
        private CloudTable _tableClient;
        private readonly IConfiguration _config;
        private HubConnection hubConnection;
        private Dictionary<string, DateTime> channelMetadata;

        public Bot(IConfiguration config)
        {
            _config = config;
            channelMetadata = new Dictionary<string, DateTime>();
        }

        public async Task Initialize()
        {
            await SetupStorage();
            await SetupTwitchClient();
            await SetupSignalRClient();
        }

        private async Task SetupStorage()
        {
            Common.CreateTableStorageAccount(_config.GetConnectionString(Constants.CONFIG_BOT_CONNSTRINGS_STORAGE));
            _tableClient = await Common.CreateTableAsync(Constants.CONFIG_BOT_STORAGE_TABLENAME);
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
            _client.OnExistingUsersDetected += async (s, e) => await Client_OnExisingUsersDetected(s, e);

            _client.Connect();

            await Task.CompletedTask;
        }

        private async Task SetupSignalRClient()
        {
            var uri = new Uri($"{_config.GetConnectionString(Constants.CONFIG_BOT_CONNSTRINGS_SIGNALR)}/{_config[Constants.CONFIG_BOT_SIGNALR_HUB_NAME]}");
            hubConnection = new HubConnectionBuilder()
                .WithUrl(uri)
                .WithAutomaticReconnect()
                .Build();

            hubConnection.On<ChannelActivityEntity>("StreamUpdate", async entity =>
            {
                var activity = (StreamActivity)Enum.Parse(typeof(StreamActivity), entity.Activity);

                switch (activity)
                {
                    case StreamActivity.StreamStarted:
                        await Start(entity.PartitionKey);
                        break;
                    case StreamActivity.StreamStopped:
                        await Stop(entity.PartitionKey);
                        break;
                    default:
                        break;
                }
            });

            hubConnection.Closed += async (error) =>
            {
                Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Closing SIGNALR Connection");
                await Task.CompletedTask;
            };

            await hubConnection.StartAsync();
        }

        public async Task Start(string channel)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Joining the channel {channel}");
            if (!_client.JoinedChannels.Any(x =>
                string.Equals(channel, x.Channel, StringComparison.InvariantCultureIgnoreCase) && _client.IsConnected))
            {
                _client.JoinChannel(channel);
            }

            if (!channelMetadata.ContainsKey(channel))
            {
                channelMetadata[channel] = DateTime.UtcNow;
            }
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

            Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Stopped the channel {channel}");

            await CalculateStreamSummaryStats(channel);
            if (channelMetadata.ContainsKey(channel))
            {
                channelMetadata.Remove(channel);
            }
        }

        private async Task CalculateStreamSummaryStats(string channel)
        {
            var channelStartDate = channelMetadata[channel];
            var query = _tableClient.CreateQuery<ChannelActivityEntity>()
                .Where(x => x.PartitionKey == channel && x.Timestamp >= channelStartDate);

            var results = query.ToList();
            string temp;

            // 1 Identify stream start(startDate)
            temp = results.OrderBy(x => x.Timestamp).FirstOrDefault().RowKey;
            var startDate = await ReformatDate(temp);

            // 2 Identify stream end(endDate)
            temp = results.OrderByDescending(x => x.Timestamp).FirstOrDefault().RowKey;
            var endDate = await ReformatDate(temp);

            // 3 Identify avg viewers (avgViewerCount) / max number of viewers (viewerCount) / number of minutesStreamed (minutesStreamed)
            // 4 Identify max number of viewers (maxViewerCount)
            // 11 Idenfity minutes streamed
            var viewers = results
                .Select(x => x.Viewer)
                .Where(x => !string.IsNullOrEmpty(x) && !string.Equals(x, channel, StringComparison.InvariantCultureIgnoreCase))
                .Distinct();
            var maxViewerCount = viewers.Count();
            var minutesStreamed = endDate.Subtract(startDate).TotalMinutes;
            var avgViewerCount = maxViewerCount / minutesStreamed;

            // 5 Identify total views

            // 6 Identify unique views

            // 7 Identify minutes watched
            var minutesWatchedLookup = new Dictionary<string, List<TimeSpan>>();
            foreach (var viewer in viewers)
            {
                var events = results
                    .Where(x => string.Equals(viewer, x.Viewer, StringComparison.InvariantCultureIgnoreCase))
                    .OrderBy(x => x.Timestamp)
                    .ToList();

                var dateValue = events.FirstOrDefault(x => string.Equals(x.Activity, StreamActivity.UserJoined.ToString())) ??
                        events.OrderBy(x => x.Timestamp).FirstOrDefault();

                DateTime viewerStartDate = startDate;
                if (dateValue != null && !string.IsNullOrEmpty(dateValue.RowKey))
                {
                    viewerStartDate = await ReformatDate(dateValue.RowKey);
                }

                // Add users who may not have left before the stream ended
                if (!events.Any(x => x.Activity == StreamActivity.UserLeft.ToString()))
                {
                    var tempMinutesWatched = endDate.Subtract(viewerStartDate);
                    minutesWatchedLookup.Add(viewer, new List<TimeSpan> { tempMinutesWatched });
                }
                else
                {
                    var userActivities = events
                        .OrderBy(x => x.Timestamp)
                        .Where(x => string.Equals(x.Activity, StreamActivity.UserJoined.ToString()) || string.Equals(x.Activity, StreamActivity.UserLeft.ToString()))
                        .Select(x => new 
                        {
                            Activity = (StreamActivity)Enum.Parse(typeof(StreamActivity), x.Activity),
                            Timestamp = ReformatDate(x.RowKey).Result
                        })
                        .ToList();

                    // if first event if UserLeft, set the userStart to the streamstart
                    var firstActivity = userActivities.FirstOrDefault();
                    if (firstActivity != null && firstActivity.Activity== StreamActivity.UserLeft)
                    {
                        var difference = userActivities.First().Timestamp.Subtract(startDate);
                        if (minutesWatchedLookup.ContainsKey(viewer))
                        {
                            var currentMinutesWatchedList = minutesWatchedLookup[viewer];
                            currentMinutesWatchedList.Add(difference);
                        }
                        else
                        {
                            minutesWatchedLookup.Add(viewer, new List<TimeSpan> { difference });
                        }
                        userActivities.Remove(firstActivity);
                    }

                    // Finds all userJoined events and their closest corresponding UserLeft events
                    foreach(var entry in userActivities.Where(x => x.Activity == StreamActivity.UserJoined))
                    {
                        var exit = userActivities.FirstOrDefault(x => x.Activity == StreamActivity.UserLeft && x.Timestamp >= entry.Timestamp);
                        if (exit == null)
                        {
                            exit = new
                            {
                                Activity = StreamActivity.UserLeft,
                                Timestamp = endDate
                            };
                        }
                        var difference = exit.Timestamp.Subtract(entry.Timestamp);

                        if (minutesWatchedLookup.ContainsKey(viewer))
                        {
                            var currentMinutesWatchedList = minutesWatchedLookup[viewer];
                            currentMinutesWatchedList.Add(difference);
                        }
                        else
                        {
                            minutesWatchedLookup.Add(viewer, new List<TimeSpan> { difference });
                        }
                    }

                }
            }
            var minutesWatched = minutesWatchedLookup.Values.Sum(x => x.Sum(y => y.TotalMinutes));

            // 8 Identify new followers
            var newFollowers = results.Where(x => string.Equals(x.Activity, StreamActivity.UserFollowed.ToString(), StringComparison.InvariantCultureIgnoreCase));
            var followerCount = newFollowers.Count();

            // 9 Identify chatter
            var chatMessages = results.Where(x => string.Equals(x.Activity, StreamActivity.MessagePosted.ToString(), StringComparison.InvariantCultureIgnoreCase));
            var chatterCount = chatMessages.Select(x => x.Viewer).Distinct().Count();

            // 10 Identify Chat messages
            var chatMessageCount = chatMessages.Count();

            // 12 Send data to the spreadsheet

            var httpClient = new HttpClient();
            var uri = new Uri(_config[Constants.CONFIG_BOT_ZAPIER_SUMMARYURL]);
            var data = new Dictionary<string, string>
            {
                {"startDate", startDate.ToString(Constants.DATETIME_FORMAT)},
                {"endDate", endDate.ToString(Constants.DATETIME_FORMAT) },
                {"avgViewer", Convert.ToString(avgViewerCount) },
                {"maxViewer",Convert.ToString(maxViewerCount) },
                {"minutesWatched", Convert.ToString(minutesWatched) },
                {"newFollowers", Convert.ToString(followerCount)},
                {"chatter", Convert.ToString(chatterCount)},
                { "chatMessages", Convert.ToString(chatMessageCount)},
                {"minutesStreamed", Convert.ToString(minutesStreamed) },
                {"channel", channel }
            };

            Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Saving data to Google sheet");
            var content = new FormUrlEncodedContent(data);
            try
            {
                var response = await httpClient.PostAsync(uri, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Something went wrong", ex);
                throw;
            }
            Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Saved Data to Google sheet");
  
        }

        private async Task<DateTime> ReformatDate(string date)
        {
            StringBuilder builder = new StringBuilder(date);
            builder = builder.Insert(4, "-");
            builder = builder.Insert(7, "-");
            builder = builder.Insert(13, ":");
            builder = builder.Insert(16, ":");

            if (builder.Length > 19)
            {
                builder = builder.Insert(19, ".");
            }

            return await Task.FromResult(DateTime.Parse(builder.ToString()));
        }

        private async Task Client_OnExisingUsersDetected(object sender, OnExistingUsersDetectedArgs e)
        {
            var date = DateTime.UtcNow;
            Console.WriteLine($"{date.ToString(CultureInfo.InvariantCulture)}: Existing users detected in {e.Channel}: {string.Join(", ",e.Users)}");
            foreach(var user in e.Users)
            {
                var entity = new ChannelActivityEntity
                {
                    PartitionKey = user,
                    RowKey = date.ToString(Constants.DATETIME_FORMAT).Replace(":", string.Empty).Replace("-", string.Empty).Replace(".", string.Empty),
                    Activity = StreamActivity.UserJoined.ToString(),
                    Viewer = e.Channel
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
                RowKey = DateTime.UtcNow.ToString(Constants.DATETIME_FORMAT).Replace(":", string.Empty).Replace("-", string.Empty).Replace(".", string.Empty),
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
            //await Common.AddMessageToQueue(_queueClient, entity);
            await Common.InsertOrMergeEntityAsync(_tableClient, entity);
        }

        private async Task Client_OnUserLeft(object sender, OnUserLeftArgs e)
        {
            Console.WriteLine(
                $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: {e.Username} left the channel {e.Channel}");
            var entity = new ChannelActivityEntity
            {
                PartitionKey = e.Channel,
                RowKey = DateTime.UtcNow.ToString(Constants.DATETIME_FORMAT).Replace(":", string.Empty).Replace("-", string.Empty).Replace(".", string.Empty),
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
                RowKey = DateTime.UtcNow.ToString(Constants.DATETIME_FORMAT).Replace(":", string.Empty).Replace("-", string.Empty).Replace(".", string.Empty),
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
                RowKey = DateTime.UtcNow.ToString(Constants.DATETIME_FORMAT).Replace(":", string.Empty).Replace("-", string.Empty).Replace(".", string.Empty),
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