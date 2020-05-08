using System.Threading.Tasks;

namespace TwitchChatBot.Shared.Interfaces
{
    public interface IStorageService
    {
        Task LoadBotSettings();
        Task SaveBotSettings();
    }
}
