using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace TwitchChatBot.Orchestrator
{
    public class Program
    {
        static async Task  Main(string[] args)
        {
            Console.WriteLine("Starting the app ");
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var configuration = builder.Build();

            var monitor = new ServiceMonitor(configuration);
            await monitor.Run();

            await Task.Delay(-1);
        }
    }
}