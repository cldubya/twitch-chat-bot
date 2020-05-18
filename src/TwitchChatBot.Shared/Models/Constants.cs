﻿namespace TwitchChatBot.Shared.Models
{
    public static class Constants
    {
        public const string APPSETTINGS_PATH = "appsettings.json";

        public const string AZURE_TABLESTORAGE = "Azure:TableStorage";
        public const string AZURE_BOTSETTINGS_TABLENAME = "Azure:BotSettings:TableName";
        public const string AZURE_BOTSETTINGS_PARTITIONKEY = "settings";

        public const string CONFIG_QUEUE_NAME_VALUE = "stream-data";
        public const string CONFIG_TABLE_NAME_VALUE = "streaming";
        public const string CONFIG_CONNSTRING_STORAGE_NAME = "Storage";

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

        public const int TWITCH_WEBHOOKS_LEASE_MAX = 864000;
        public const string TWITCH_WEBHOOKS_SUBSCRIPTION_URL = "https://api.twitch.tv/helix/webhooks/hub";
    }
}
