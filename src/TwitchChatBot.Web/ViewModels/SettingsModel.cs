using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TwitchChatBot.Web.ViewModels
{
    public class SettingsModel
    {
        [Required]
        public string BotUsername { get; set; }
        [Required]
        [PasswordPropertyText]
        public string BotPassword { get; set; }

        public List<string> Channels { get; set; }

    }
}
