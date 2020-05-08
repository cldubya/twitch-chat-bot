using Microsoft.Azure.Cosmos.Table;

namespace TwitchChatBot.Shared.Models
{
    public class BotSettingsEntity : TableEntity
    {
        private const string PARTITIONKEY = Constants.AZURE_BOTSETTINGS_PARTITIONKEY;

        public BotSettingsEntity()
        {

        }

        public BotSettingsEntity(string dateTime)
        {
            PartitionKey = PARTITIONKEY;
            RowKey = dateTime;
        }
    }
}
