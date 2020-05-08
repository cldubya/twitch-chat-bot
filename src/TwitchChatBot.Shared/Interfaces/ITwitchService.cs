using System.Threading.Tasks;

namespace TwitchChatBot.Shared.Interfaces
{
    public interface ITwitchService
    {
        bool IsInitialized { get; set; }

        Task CreateTwitchClient(string username, string password);

        Task DisconnectFromTwitch();
    }
}
