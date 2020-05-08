using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using TwitchChatBot.Shared.Models;

namespace TwitchChatBot.DataExporter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var _connString =
                "DefaultEndpointsProtocol=https;AccountName=twitchstreamstrg;AccountKey=QCuMzB6R92G/Qwscn0E7LGJFZem7y8ZdHCBA7dlY9CwSFpTWMJEpvsnwPfJJbhu46KY2cDGeC62wclrFEkerMQ==;EndpointSuffix=core.windows.net";
            var account = Common.CreateStorageAccountFromConnectionString(_connString);
            var table = await Common.CreateTableAsync();
            var results = await Common.QueryAsync(table);

            var data = results.Select(x => string.Join(',', x.PartitionKey,x.Activity, x.Viewer, x.Timestamp)).ToList();
            
            File.AppendAllLines("C:\\Code\\test.csv",data);
            
        }
    }

    public class Common
    {
        private static string _connectionString = "UseDevelopmentStorage=true";

        public static CloudStorageAccount CreateStorageAccountFromConnectionString(
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

        public static async Task<CloudTable> CreateTableAsync(string tableName = "streaming")
        {
            string storageConnectionString = _connectionString;

            // Retrieve storage account information from connection string.
            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);

            // Create a table client for interacting with the table service
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());

            Console.WriteLine("Create a Table for the demo");

            // Create a table client for interacting with the table service 
            CloudTable table = tableClient.GetTableReference(tableName);
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
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                ChannelActivityEntity insertedActivity = result.Result as ChannelActivityEntity;

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

        public static async Task<List<ChannelActivityEntity>> QueryAsync(CloudTable table)
        {
            var query = new TableQuery<ChannelActivityEntity>();
            var result = table.ExecuteQuery(query).ToList();
            return await Task.FromResult(result);
        }
    }
}