using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Models;
using TwitchChatBot.Shared.Services;

[assembly: FunctionsStartup(typeof(TwitchChatBot.Fx.Startup))]
namespace TwitchChatBot.Fx
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddHttpClient(Constants.FX_HTTPCLIENT_NAME,
                opts =>
                {
                    opts.DefaultRequestHeaders.Clear();
                    opts.BaseAddress = new Uri("https://hooks.zapier.com");
                });

            builder.Services.AddHttpClient(Constants.FX_TWITCH_WEBHOOKS_NAME,
                opts =>
                {
                    opts.DefaultRequestHeaders.Clear();
                    opts.BaseAddress = new Uri("https://api.twitch.tv/");
                });


            builder.Services.AddSingleton<IStorageService, AzureTableStorageService>();

            var localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            var azureRoot = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";

            var actualRoot = localRoot ?? azureRoot;

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(actualRoot)
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
            IConfiguration configuration = configBuilder.Build();
            builder.Services.AddSingleton(configuration);

        }
    }
}
