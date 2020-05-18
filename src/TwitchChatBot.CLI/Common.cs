using Azure.Storage.Queues;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.CLI
{
    public static class Common
    {
        private static string _connectionString = "UseDevelopmentStorage=true";

        public static CloudStorageAccount CreateTableStorageAccount(
            string storageConnectionString = "UseDevelopmentStorage=true")
        {
            _connectionString = storageConnectionString;
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(_connectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine(
                    $"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }

        public static async Task<QueueClient> CreateQueue(string queueName = "streaming")
        {
            var queueClient = new QueueClient(connectionString: _connectionString, queueName);
            await queueClient.CreateIfNotExistsAsync();
            return queueClient;
        }

        public static async Task AddMessageToQueue(QueueClient queue, ChannelActivityEntity entity)
        {
            var json = JsonSerializer.Serialize(entity, typeof(ChannelActivityEntity), options: new JsonSerializerOptions { });
            await queue.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json)));
        }

        public static async Task<CloudTable> CreateTableAsync(string tableName = "streaming")
        {
            var storageConnectionString = _connectionString;

            // Retrieve storage account information from connection string.
            var storageAccount = CreateTableStorageAccount(storageConnectionString);

            // Create a table client for interacting with the table service
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Connecting to Table {tableName}");

            // Create a table client for interacting with the table service 
            var table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Created Table named {tableName}");
            }
            else
            {
                Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Table {tableName} already exists");
            }
            return table;
        }

        public static async Task<ChannelActivityEntity> InsertOrMergeEntityAsync(CloudTable table,
            ChannelActivityEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            try
            {
                // Create the InsertOrReplace table operation
                var insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Adding entity {entity} to the table {table.Name}");
                var result = await table.ExecuteAsync(insertOrMergeOperation);
                var insertedActivity = result.Result as ChannelActivityEntity;

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure Cosmos DB
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Request Charge of InsertOrMerge Operation: {result.RequestCharge}");
                    Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: Added entity {entity} to the table {table.Name}");

                }

                return insertedActivity;
            }
            catch (StorageException e)
            {
                Console.WriteLine($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)}: An exception has occured", e);
                throw;
            }
        }
    }
}