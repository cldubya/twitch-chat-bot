using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchChatBot.Shared.Services
{
    public class TwitchService : ITwitchService
    {
        protected readonly ILogger<ITwitchService> _logger;
        public bool IsInitialized { get; set; }
        private TwitchClient _twitchClient;

        public TwitchService(ILogger<ITwitchService> logger)
        {
            _logger = logger;
        }

        public Task CreateTwitchClient(string username, string password)
        {
            _logger.LogInformation("Creating the Twitch Client");
            if (_twitchClient == null)
            {
                InitializeTwitchClient(username, password);
                SetupTwitchEvents();
            }
            _logger.LogInformation("Created the Twitch Client");

            return Task.CompletedTask;
        }

        public Task DisconnectFromTwitch()
        {
            throw new System.NotImplementedException();
        }

        private void InitializeTwitchClient(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException(nameof(username));
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException(nameof(password));
            }

            var credentials = new ConnectionCredentials(username, password);
            var options = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };

            var webSocketClient = new WebSocketClient(options);
            _twitchClient = new TwitchClient(webSocketClient);
            _twitchClient.Initialize(credentials);
        }


        private void SetupTwitchEvents()
        {
            _twitchClient.OnMessageReceived += TwitchClient_OnMessageReceived;
            _twitchClient.OnUserJoined += TwitchClient_OnUserJoined;
            _twitchClient.OnUserLeft += TwitchClient_OnUserLeft;
        }

        private void TwitchClient_OnUserLeft(object sender, TwitchLib.Client.Events.OnUserLeftArgs e)
        {
            throw new NotImplementedException();
        }

        private void TwitchClient_OnUserJoined(object sender, TwitchLib.Client.Events.OnUserJoinedArgs e)
        {
            throw new NotImplementedException();
        }

        private void TwitchClient_OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
