using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Models;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.FollowerService;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace TwitchChatBot.CLI
{
    public class StreamMonitorService : IDisposable
    {
        private readonly LiveStreamMonitorService _liveStreamMonitorService;
        private readonly FollowerService _followerService;
        private readonly Bot _bot;

        public StreamMonitorService(IConfiguration config, Bot bot)
        {
            var twitchApi = new TwitchAPI();
            twitchApi.Settings.ClientId = config[Constants.CONFIG_TWITCH_CLIENTID];
            twitchApi.Settings.AccessToken = config[Constants.CONFIG_TWITCH_ACCESSTOKEN];

            _bot = bot;

            _liveStreamMonitorService = new LiveStreamMonitorService(twitchApi);
            _followerService = new FollowerService(twitchApi);
        }

        public async Task Initialize(List<string> channels)
        {
            var liveStreamMonitorTask = Task.Run(() => 
            {
                _liveStreamMonitorService.SetChannelsByName(channels);

                _liveStreamMonitorService.OnServiceStarted +=
                    async (s, e) => await LiveStreamMonitorServiceOnOnServiceStarted(s, e);
                _liveStreamMonitorService.OnServiceStopped +=
                    async (s, e) => await LiveStreamMonitorServiceOnOnServiceStopped(s, e);
                _liveStreamMonitorService.OnStreamOnline +=
                    async (s, e) => await LiveStreamMonitorServiceOnOnStreamOnline(s, e);
                _liveStreamMonitorService.OnStreamOffline +=
                    async (s, e) => await LiveStreamMonitorServiceOnOnStreamOffline(s, e);

                _liveStreamMonitorService.Start();
            });

            var followerServiceTask = Task.Run(() => 
            {
                _followerService.SetChannelsByName(channels);

                _followerService.OnNewFollowersDetected += async(s,e) => await FollowerService_OnNewFollowersDetected(s,e);
            });
           

            

            await Task.CompletedTask;
        }

        private async Task FollowerService_OnNewFollowersDetected(object sender, OnNewFollowersDetectedArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow}: Stream Online: {e.Channel}");
            var followers = e.NewFollowers.Select(x => x.ToUserName).Distinct().ToList();
            await _bot.AddNewFollowers(e.Channel, followers);
        }

        private async Task LiveStreamMonitorServiceOnOnServiceStarted(object sender, OnServiceStartedArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow}: Live Stream Service Monitor Started");
            await Task.CompletedTask;
        }

        private async Task LiveStreamMonitorServiceOnOnServiceStopped(object sender, OnServiceStoppedArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow}: Live Stream Service Monitor Stopped");
            await Task.CompletedTask;
        }

        private async Task LiveStreamMonitorServiceOnOnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow}: Stream Online: {e.Channel}");
            await _bot.Start(e.Channel);
        }

        private async Task LiveStreamMonitorServiceOnOnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow}: Stream Offline: {e.Channel}");
            await _bot.Stop(e.Channel);
        }

        public void Dispose()
        {
            _liveStreamMonitorService.Stop();
        }
    }
}