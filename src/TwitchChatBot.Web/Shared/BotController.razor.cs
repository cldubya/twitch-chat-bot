using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Enums;

namespace TwitchChatBot.Web.Shared
{
    public partial class BotController
    {
        //TODO: Fix the BotController IsEnabled control
        [Parameter]
        public BotState State { get; set; }
        protected bool IsEnabled { get; set; }
        [Inject]
        protected IBotService BotService { get; set; }
        [Inject]
        protected ILogger<BotController> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await BotService.LoadState();
        }

        protected async Task StartBot()
        {
            Logger.LogInformation("Starting the bot");
            await BotService.StartBot();
            await BotService.ChangeBotState(BotState.Started);
            Logger.LogInformation("Started the bot");

            StateHasChanged();
        }

        protected async Task StopBot()
        {
            Logger.LogInformation("Stopping the bot");
            await BotService.StopBot();
            await BotService.ChangeBotState(BotState.Stopped);
            Logger.LogInformation("Stopped the bot");
        }
    }
}
