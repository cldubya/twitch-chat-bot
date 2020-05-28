using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Models;
using TwitchLib.Api.V5.Models.Channels;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace TwitchChatBot.Shared.Services
{
    public class TwitchService : ITwitchService
    {
        public bool IsInitialized { get; set; }

        protected string AccessToken { get; private set; } = "";
        protected string RefreshToken { get; private set; } = "";
        protected string AppAccessToken { get; private set; } = "";
        protected List<TwitchUser> TwitchUsers { get; private set; }

        protected readonly ILogger<ITwitchService> _logger;
        protected readonly IHttpClientFactory _httpClientFactory;

        private TwitchClient _twitchClient;
        internal HttpClient _httpClient;
        
        public List<string> Channels
        {
            get
            {
                return TwitchUsers?.Select(x => x.Id).ToList();
            }
        }

        public TwitchService(IHttpClientFactory httpClientFactory, ILogger<ITwitchService> logger)
        {
            _httpClientFactory = httpClientFactory;
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

        public Task SetTokens(string accessToken, string refreshToken, string appAccessToken, string clientId)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                AccessToken = accessToken;
            }

            if (!string.IsNullOrEmpty(refreshToken))
            {
                RefreshToken = refreshToken;
            }

            if (!string.IsNullOrEmpty(appAccessToken))
            {
                AppAccessToken = appAccessToken;
            }

            var temp = _httpClientFactory.CreateClient();
            temp.DefaultRequestHeaders.Clear();
            temp.DefaultRequestHeaders.Add("Client-ID", clientId);

            _httpClient = temp;
    
            return Task.CompletedTask;
        }

        public async Task SubscribeToChannelEvents(List<string> channels)
        {
            await UpdateFollowSubscription(channels, true);
            await UpdateStreamChangeSubscription(channels, true);
        }

        public async Task UnsubscribeFromChannelEvents(List<string> channels)
        {
            await UpdateFollowSubscription(channels, false);
            await UpdateStreamChangeSubscription(channels, false);
        }

        public async Task GetCurrentSubscriptions()
        {
            var url = new Uri("https://api.twitch.tv/helix/webhooks/subscriptions");
            var response = await MakeHttpGetRequest(url, AppAccessToken);
            var data = await response.Content.ReadAsStringAsync();
            //TODO: Convert JSON response for current subscriptions to object
        }

        // Based off of Twitch Documentation https://dev.twitch.tv/docs/api/reference#get-users
        public async Task LoadChannelData(List<string> channels)
        {
            if (TwitchUsers != null && TwitchUsers.Count > 0)
            {
                await Task.CompletedTask;
                return;
            }
            if (TwitchUsers == null)
            {
                TwitchUsers = new List<TwitchUser>();
            }

            // TODO: Convert Twitch User URL to constant
            var url = new Uri("https://api.twitch.tv/helix/users");
            var channelPaths = new List<string>();
            foreach(var channel in channels)
            {
                channelPaths.Add($"login={channel}");
            }

            var uriBuilder = new UriBuilder(url);
            uriBuilder.Query = string.Join("&", channelPaths).TrimStart('&');
            var response = await MakeHttpGetRequest(uriBuilder.Uri, AccessToken);


            // TODO: Convert JSON Parser to use System.Text.Json
            var jobject = JObject.Parse(await response.Content.ReadAsStringAsync());
            TwitchUsers = JsonConvert.DeserializeObject<List<TwitchUser>>(jobject["data"].ToString());
        }

        private async Task UpdateFollowSubscription(List<string> channels, bool isSubscribed)
        {
            foreach (var channel in channels)
            {
                var selected = TwitchUsers.FirstOrDefault(user => string.Equals(channel, user.LoginName, StringComparison.InvariantCultureIgnoreCase));
                // TODO: Inject current domain name into UpdateFollowSubscription methods 

                var request = new TwitchWebhookRequest
                {
                    Callback = $"https://twitchchatbotfx20200511150805.azurewebsites.net/api/subscription/followers/{channel}",
                    Mode = isSubscribed ? "subscribe" : "unsubscribe",
                    Topic = $"https://api.twitch.tv/helix/users/follows?first=1&to_id={selected.Id}",
                    Lease = 864000
                };

                
                var url = new Uri(Constants.TWITCH_WEBHOOKS_SUBSCRIPTION_URL);
                var response = await MakeHttpPostRequest( url, request, AccessToken);
            }
        }

        private async Task<HttpResponseMessage> MakeHttpPostRequest(Uri url, TwitchWebhookRequest request, string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.Default });
            var formData = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, formData);
            response.EnsureSuccessStatusCode();
            return response;
        }


        private async Task<HttpResponseMessage> MakeHttpGetRequest(Uri url, string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return response;
        }

        private async Task UpdateStreamChangeSubscription(List<string> channels, bool isSubscribed)
        {
            foreach (var channel in channels)
            {
                var selected = TwitchUsers.FirstOrDefault(user => string.Equals(channel, user.LoginName, StringComparison.InvariantCultureIgnoreCase));
                // TODO: Inject callback url into UpdateSubscription methods 

                var request = new TwitchWebhookRequest
                {
                    Callback = $"https://twitchchatbotfx20200511150805.azurewebsites.net/api/subscription/streams/{channel}",
                    Mode = isSubscribed ? "subscribe" : "unsubscribe",
                    Topic = $"https://api.twitch.tv/helix/streams?user_id={selected.Id}",
                    Lease = 864000
                };

                var url = new Uri(Constants.TWITCH_WEBHOOKS_SUBSCRIPTION_URL);
                var response = await MakeHttpPostRequest(url, request, AccessToken);
            }
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

            _twitchClient.Connect();
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
