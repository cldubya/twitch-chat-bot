namespace TwitchChatBot.Shared.Models
{
    public static class Constants
    {
        public const string APPSETTINGS_PATH = "appsettings.json";

        public const string AZURE_TABLESTORAGE = "Azure:TableStorage";
        public const string AZURE_BOTSETTINGS_TABLENAME = "Azure:BotSettings:TableName";
        public const string AZURE_BOTSETTINGS_PARTITIONKEY = "settings";

        public const string DATETIME_FORMAT = "o";

        public const string CONFIG_BOT_ZAPIER_SUMMARYURL = "Values:Zapier:StreamSummaryUrl";
        public const string CONFIG_BOT_CONNSTRINGS_STORAGE = "Storage";
        public const string CONFIG_BOT_STORAGE_TABLENAME = "Values:Storage:TableName";
        public const string CONFIG_BOT_CONNSTRINGS_SIGNALR = "SignalR";
        public const string CONFIG_BOT_SIGNALR_HUB_NAME = "Values:SignalR:HubName";

        public const string CONFIG_SIGNALR_URL = "Values:SignalRUrl";
        public const string CONFIG_TWITCH_ACCESSTOKEN = "Values:Twitch:AccessToken";
        public const string CONFIG_TWITCH_BOTUSERNAME = "Values:Twitch:BotUsername";
        public const string CONFIG_TWITCH_BOTPASSWORD = "Values:Twitch:BotPassword";
        public const string CONFIG_TWITCH_CHANNELS = "Values:Twitch:Channels";
        public const string CONFIG_TWITCH_CLIENTID = "Values:Twitch:ClientId";
        public const string CONFIG_TWITCH_PASSWORD = "Values:Twitch:Password";
        public const string CONFIG_TWITCH_CLIENTSECRET = "Values:Twitch:ClientSecret";
        public const string CONFIG_TWITCH_USERNAME = "Values:Twitch:UserName";
        public const string CONFIG_ZAPIER_URL = "Values:ZapierUrl";

        public const string FX_HTTPCLIENT_NAME = "zapier";
        public const string FX_TWITCH_WEBHOOKS_NAME = "twitchWebhooks";
        public const string FX_CONFIG_FOLLOWERS_QUEUE_NAME_VALUE = "followers-data";
        public const string FX_CONFIG_STREAM_QUEUE_NAME_VALUE = "streamdata";
        public const string FX_CONFIG_TABLE_NAME_VALUE = "streaming";
        public const string FX_CONFIG_CONNSTRING_STORAGE_NAME = "FxStorage";
        public const string FX_CONFIG_SIGNALR_HUB_VALUE = "stream-bot";
        public const string CONFIG_FX_SIGNALR_HUBNAME = "Values:SignalRHubName";


        public const int TWITCH_WEBHOOKS_LEASE_MAX = 864000;
        public const string TWITCH_WEBHOOKS_SUBSCRIPTION_URL = "https://api.twitch.tv/helix/webhooks/hub";
    }
}
