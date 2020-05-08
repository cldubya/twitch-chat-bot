using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TwitchChatBot.Shared.Models;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace TwitchChatBot.CLI
{
    public class StreamMonitorService : IDisposable
    {
        private readonly IConfiguration _config;
        private readonly LiveStreamMonitorService _liveStreamMonitorService;
        private Bot _bot;

        public StreamMonitorService(IConfiguration config, Bot bot)
        {
            _config = config;
            var twitchApi = new TwitchAPI();
            twitchApi.Settings.ClientId = config[Constants.CONFIG_TWITCH_CLIENTID];
            twitchApi.Settings.AccessToken = config[Constants.CONFIG_TWITCH_ACCESSTOKEN];

            _bot = bot;

            _liveStreamMonitorService = new LiveStreamMonitorService(twitchApi);
        }

        public async Task Initialize(List<string> channels)
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

            await Task.CompletedTask;
        }

        private async Task LiveStreamMonitorServiceOnOnServiceStarted(object? sender, OnServiceStartedArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString()}: Live Stream Service Monitor Started");
            await Task.CompletedTask;
        }

        private async Task LiveStreamMonitorServiceOnOnServiceStopped(object? sender, OnServiceStoppedArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString()}: Live Stream Service Monitor Stopped");
            await Task.CompletedTask;
        }

        private async Task LiveStreamMonitorServiceOnOnStreamOnline(object? sender, OnStreamOnlineArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString()}: Stream Online: {e.Channel}");
            await _bot.Start(e.Channel);
        }

        private async Task LiveStreamMonitorServiceOnOnStreamOffline(object? sender, OnStreamOfflineArgs e)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString()}: Stream Offline: {e.Channel}");
            await _bot.Stop(e.Channel);
        }

        public void Dispose()
        {
            _liveStreamMonitorService.Stop();
        }
    }
}