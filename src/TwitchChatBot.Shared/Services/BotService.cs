using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Enums;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Models;
using System.Linq;

namespace TwitchChatBot.Shared.Services
{
    public class BotService : IBotService
    {
        public List<string> Channels { get; set; }
        private readonly IConfiguration _configuration;
        private readonly IStorageService _storageService;
        private readonly ILogger<IBotService> _logger;
        private readonly ITwitchService _twitchService;

        public BotState CurrentState { get; private set; }

        public BotService(ITwitchService twitchService, IConfiguration configuration, ILogger<IBotService> logger, IStorageService storageService)
        {
            _twitchService = twitchService;
            _configuration = configuration;
            _logger = logger;
            _storageService = storageService;
        }

        public Task LoadState()
        {
            _logger.LogInformation("Loading the bot state");

            _logger.LogInformation("Loaded the bot state");

            return Task.CompletedTask;
        }

        public Task ChangeBotState(BotState desiredState)
        {
            _logger.LogInformation($"Changing bot state. Current State: {Enum.GetName(typeof(BotState), CurrentState)}; Desired State: {Enum.GetName(typeof(BotState), desiredState)}");
            CurrentState = desiredState;
            _logger.LogInformation($"Changed Bot State. Current State: {Enum.GetName(typeof(BotState), CurrentState)}");
            return Task.CompletedTask;
        }

        public Task StartBot()
        {
            if (!_twitchService.IsInitialized)
            {
                // todo: read username and password from config settings
                var task = _twitchService.CreateTwitchClient("twiliochatbot", "oauth:3ehb2hdqt9qm3ib6khcte4dkcyhss3");
            }

            LoadChannels();

            return Task.CompletedTask;
        }

        public Task StopBot()
        {
            throw new NotImplementedException();
        }

        private Task LoadChannels()
        {
            var channelSettings = _configuration.GetSection(Constants.CONFIG_TWITCH_CHANNELS);
            

            if (Channels == null)
            {
                Channels = new List<string>();
            }

            Channels = channelSettings.Get<List<string>>();

            return Task.CompletedTask;
        }
    }
}
