using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Models;
using TwitchLib.Api.V5;

namespace TwitchChatBot.Web.v2.Pages
{
    public partial class Subscriptions
    {
        [Inject]
        protected ITwitchService TwitchService { get; set; }
        [Inject]
        protected ILogger<Subscriptions> Logger { get; set; }
        [Inject]
        protected IConfiguration Config { get; set; }
        [Inject]
        protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                await TwitchService.LoadChannelData(Config.GetSection(Constants.CONFIG_TWITCH_CHANNELS).Get<List<string>>());
                await TwitchService.GetCurrentSubscriptions();
            }
        }

        public async Task StartSubscription()
        {
            Logger.LogInformation($"{DateTime.UtcNow}: Starting Subscription");

            var channels = Config.GetSection(Constants.CONFIG_TWITCH_CHANNELS).Get<List<string>>();
            await TwitchService.SubscribeToChannelEvents(channels);

            Logger.LogInformation($"{DateTime.UtcNow}: Started Subscription");
        }

        public async Task StopSubscription()
        {
            Logger.LogInformation($"{DateTime.UtcNow}: Stopping Subscription");

            var channels = Config.GetSection(Constants.CONFIG_TWITCH_CHANNELS).Get<List<string>>();
            await TwitchService.UnsubscribeFromChannelEvents(channels);

            Logger.LogInformation($"{DateTime.UtcNow}: Stopped Subscription");
        }
    }
}
