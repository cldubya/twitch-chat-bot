using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace TwitchChatBot.Orchestrator
{
    internal class ServiceMonitor
    { 
        private readonly Dictionary<string,int> _botProcessDictionary = new Dictionary<string, int>();
        private readonly IConfiguration _config;
        private readonly LiveStreamMonitorService _liveStreamMonitorService;
        private readonly string _processFileName;

        public ServiceMonitor(IConfiguration config)
        {
            _config = config;
            var channels = _config.GetSection("Values:Twitch:Channels").Get<List<string>>();

            _processFileName = $@"{_config["Values:BotAssembly:Directory"]}\{_config["Values:BotAssembly:Name"]}";

            var twitchApi = new TwitchAPI();
            twitchApi.Settings.ClientId = config["Values:Twitch:ClientId"];
            twitchApi.Settings.AccessToken = config["Values:Twitch:AccessToken"];

            _liveStreamMonitorService = new LiveStreamMonitorService(twitchApi);
            _liveStreamMonitorService.SetChannelsByName(channels);

            _liveStreamMonitorService.OnStreamOnline +=
                async (s, e) => await LiveStreamMonitorServiceOnOnStreamOnline(s, e);
            _liveStreamMonitorService.OnStreamOffline +=
                async (s, e) => await LiveStreamMonitorServiceOnOnStreamOffline(s, e);
        }
        
        private Task LiveStreamMonitorServiceOnOnStreamOnline(object? sender, OnStreamOnlineArgs e)
        {
            Console.WriteLine($"Stream Online: {e.Channel}");
            // Call process channel runningdirectory
            var processStartInfo = new ProcessStartInfo
            {
                FileName = _processFileName,
                Arguments = $"{e.Channel} {_config["Values:BotAssembly:Directory"]}"
            };

            var process = new Process {StartInfo = processStartInfo};
            if (!process.Start())
            {
                throw new ApplicationException($"Process {process.ProcessName} unable to start");
            }
            
            _botProcessDictionary.Add(e.Channel,process.Id);
            return Task.CompletedTask;
        }

        private Task LiveStreamMonitorServiceOnOnStreamOffline(object? sender, OnStreamOfflineArgs e)
        {
            Console.WriteLine($"Stream offline: {e.Channel}");
            if (!_botProcessDictionary.ContainsKey(e.Channel))
            {
                return Task.CompletedTask;
            }

            var pid = _botProcessDictionary[e.Channel];
            var process = Process.GetProcessById(pid);
            
            process.Kill();
            process.WaitForExit();
            
            return Task.CompletedTask;
        }

        public async Task Run()
        {
            _liveStreamMonitorService.Start();
            await Task.CompletedTask;
        }
    }
}