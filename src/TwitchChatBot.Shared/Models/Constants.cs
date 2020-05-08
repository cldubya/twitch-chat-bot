using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwitchChatBot.Shared.Models
{
    public static class Constants
    {
        public const string APPSETTINGS_PATH = "appsettings.json";

        public const string AZURE_TABLESTORAGE = "Azure:TableStorage";
        public const string AZURE_BOTSETTINGS_TABLENAME = "Azure:BotSettings:TableName";
        public const string AZURE_BOTSETTINGS_PARTITIONKEY = "settings";

        public const string CONFIG_TWITCH_ACCESSTOKEN = "Values:Twitch:AccessToken";
        public const string CONFIG_TWITCH_BOTUSERNAME = "Values:Twitch:BotUsername";
        public const string CONFIG_TWITCH_BOTPASSWORD = "Values:Twitch:BotPassword";
        public const string CONFIG_TWITCH_CHANNELS = "Values:Twitch:Channels";
        public const string CONFIG_TWITCH_CLIENTID = "Values:Twitch:ClientId";
        public const string CONFIG_TWITCH_PASSWORD = "Values:Twitch:Password";
        public const string CONFIG_TWITCH_SECRET = "Values:Twitch:Secret";
        public const string CONFIG_TWITCH_USERNAME = "Values:Twitch:UserName";
        public const string CONFIG_ZAPIER_URL = "Values:ZapierUrl";

        public const string CONNECTIONSTRINGS_STORAGE = "ConnectionStrings:Storage";

        public const string FX_HTTPCLIENT_NAME = "zapier";

        public const string QUEUE_NAME = "stream-data";
        public const string QUEUE_STORAGE_CONNECTION = "Storage";
    }
}
