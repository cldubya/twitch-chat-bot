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

        public AzureTableStorageService(ILogger<IStorageService> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;

            if (string.IsNullOrEmpty(_connectionString))
            {
                _connectionString = _configuration.GetConnectionString(Constants.FX_CONFIG_CONNSTRING_STORAGE_NAME);
            }
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
            throw new NotImplementedException();
        }

        public async Task AddDataToStorage(ChannelActivityEntity entity)
        {
            try
            {
                if (_tableClient == null)
                {
                    CreateTableClient(_connectionString);
                }

                var insertOperation = TableOperation.InsertOrMerge(entity);
                var table = _tableClient.GetTableReference(Constants.FX_CONFIG_TABLE_NAME_VALUE);
                var result = await table.ExecuteAsync(insertOperation);
            }
            catch (StorageException ex)
            {
                _logger.LogError($"{DateTime.UtcNow}: An exception has occurred: {ex.Message}", ex);
                throw;
            }
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
