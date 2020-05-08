using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Interfaces;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.Shared.Services
{
    public class AzureTableStorageService : IStorageService
    {
        private CloudTableClient _tableClient;
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IStorageService> _logger;

        public AzureTableStorageService(ILogger<IStorageService> logger, IConfiguration configuration, string connectionString = "")
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                _connectionString = connectionString;
            }
            _configuration = configuration;
            _logger = logger;
        }

        public async Task LoadBotSettings()
        {
            _logger.LogInformation("Loading bot settings");
            CreateTableClient(_connectionString ?? _configuration.GetConnectionString(Constants.AZURE_TABLESTORAGE));

            var table = _tableClient.GetTableReference(_configuration[Constants.AZURE_BOTSETTINGS_TABLENAME]);

            try
            {
                var settings = table.CreateQuery<BotSettingsEntity>().Where(entity => entity.PartitionKey == Constants.AZURE_BOTSETTINGS_PARTITIONKEY)
                    .Select(result => new BotSettingsEntity { PartitionKey = Constants.AZURE_BOTSETTINGS_PARTITIONKEY, RowKey = result.RowKey }).FirstOrDefault();
            }
            catch
            {
                await table.CreateIfNotExistsAsync();
            }
            _logger.LogInformation("Loaded bot settings");
        }

        public Task SaveBotSettings()
        {
            throw new System.NotImplementedException();
        }

        private void CreateTableClient(string connectionString)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                _tableClient = storageAccount.CreateCloudTableClient();
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception has occurred", ex);
            }
        }
    }
}
