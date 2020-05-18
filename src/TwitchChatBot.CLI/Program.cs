using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.CLI
{
    // run process with arguments {channelName} and {currentDirectory}
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            // Load config
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(Constants.APPSETTINGS_PATH, optional: false, reloadOnChange: true)
                .Build();

            // Get Channels to monitor
            var channels = config.GetSection(Constants.CONFIG_TWITCH_CHANNELS).Get<List<string>>();

            // Start the bot
            var bot = new Bot(config);
            await bot.Initialize();

            // Start the live stream service
            /*var svc = new StreamMonitorService(config, bot);
            await svc.Initialize(channels);*/

            await Task.Delay(-1);
        }
    }



}