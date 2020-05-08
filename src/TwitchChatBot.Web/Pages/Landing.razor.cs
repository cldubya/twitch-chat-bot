using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Enums;
using TwitchChatBot.Shared.Interfaces;

namespace TwitchChatBot.Web.Pages
{
    public partial class Landing
    {
        [Inject]
        protected ILogger<Landing> Logger { get; set; }

        [Inject]
        protected IBotService BotService { get; set; }
        [Inject]
        protected ITwitchService TwitchService { get; set; }

        private List<string> _channels;


        protected async Task StartBot()
        {
            Logger.LogInformation("Starting the bot");
            await BotService.StartBot();
            await BotService.ChangeBotState(BotState.Started);
            Logger.LogInformation("Started the bot");

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
