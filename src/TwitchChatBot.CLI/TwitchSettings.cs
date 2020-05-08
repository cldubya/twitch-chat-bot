using System.Collections.Generic;

namespace TwitchChatBot.CLI
{
    public class TwitchSettings
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ClientId { get; set; }
        public List<string> Channels { get; set; }
    }
}