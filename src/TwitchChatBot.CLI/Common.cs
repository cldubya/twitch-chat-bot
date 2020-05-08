using Azure.Storage.Queues;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Threading.Tasks;
using TwitchChatBot.Shared.Models;
using System.Text.Json;

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
                Console.WriteLine(
                    "Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine(
                    "Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
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
            var json = JsonSerializer.Serialize(entity, typeof(ChannelActivityEntity), options: new JsonSerializerOptions {  });
            await queue.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json)));
        }

        public static async Task<CloudTable> CreateTableAsync(string tableName = "streaming")
        {
            var storageConnectionString = _connectionString;

            // Retrieve storage account information from connection string.
            var storageAccount = CreateTableStorageAccount(storageConnectionString);

            // Create a table client for interacting with the table service
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            Console.WriteLine("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            var table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", tableName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", tableName);
            }

            Console.WriteLine();
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
                var result = await table.ExecuteAsync(insertOrMergeOperation);
                var insertedActivity = result.Result as ChannelActivityEntity;

                // Get the request units consumed by the current operation. RequestCharge of a TableResult is only applied to Azure Cosmos DB
                if (result.RequestCharge.HasValue)
                {
                    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                }

                return insertedActivity;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }
    }
}