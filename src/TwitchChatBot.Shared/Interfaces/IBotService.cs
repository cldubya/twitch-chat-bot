using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Enums;

namespace TwitchChatBot.Shared.Interfaces
{
    public interface IBotService
    {
        List<string> Channels { get; set; }
        BotState CurrentState { get; }
        Task ChangeBotState(BotState desiredState);
        Task LoadState();

        Task StartBot();
        Task StopBot();
    }
}
