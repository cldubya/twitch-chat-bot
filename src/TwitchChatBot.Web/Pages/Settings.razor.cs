using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Models;
using TwitchChatBot.Web.ViewModels;


namespace TwitchChatBot.Web.Pages
{
    public partial class Settings
    {
        [Inject]
        private IConfiguration Configuration { get; set; }
        private SettingsModel _model;

        protected override Task OnInitializedAsync()
        {
            if (_model == null)
            {
                _model = new SettingsModel();
            }

            _model.BotUsername = Configuration[Constants.CONFIG_TWITCH_BOTUSERNAME];
            _model.BotPassword = Configuration[Constants.CONFIG_TWITCH_BOTPASSWORD];
            _model.Channels = Configuration.GetSection(Constants.CONFIG_TWITCH_CHANNELS).GetChildren().ToList().Select(x => x.Value).ToList();
;

            return Task.CompletedTask;
        }

        protected Task SaveSettings()
        {
            UpdateTwitchSettings();
            return Task.CompletedTask;
        }

        private void UpdateTwitchSettings()
        {
            Configuration[Constants.CONFIG_TWITCH_BOTUSERNAME] = _model.BotUsername;
            Configuration[Constants.CONFIG_TWITCH_BOTPASSWORD] = _model.BotPassword;
        }
    }
}
