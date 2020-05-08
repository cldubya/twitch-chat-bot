using Microsoft.Azure.Cosmos.Table;

namespace TwitchChatBot.Shared.Models
{
    public class ChannelActivityEntity : TableEntity
    {
        public string Viewer { get; set; }
        public string Activity { get; set; }

        public ChannelActivityEntity()
        {

        }
    }
}
