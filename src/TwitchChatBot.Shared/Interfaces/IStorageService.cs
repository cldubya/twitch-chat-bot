using System.Threading.Tasks;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.Shared.Interfaces
{
    public interface IStorageService
    {
        Task LoadBotSettings();
        Task SaveBotSettings();

        Task AddDataToStorage(ChannelActivityEntity entity);
    }
}
