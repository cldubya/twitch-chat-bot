using System.Collections.Generic;
using System.Threading.Tasks;

namespace TwitchChatBot.Shared.Interfaces
{
    public interface ITwitchService
    {
        bool IsInitialized { get; set; }

        List<string> Channels { get; }

        Task SetTokens(string accessToken, string refreshToken, string appAccessToken, string clientId);

        Task LoadChannelData(List<string> channels);

        Task CreateTwitchClient(string username, string password);

        Task GetCurrentSubscriptions();

        Task SubscribeToChannelEvents(List<string> channel);
        Task UnsubscribeFromChannelEvents(List<string> channel);


        Task DisconnectFromTwitch();
    }
}
