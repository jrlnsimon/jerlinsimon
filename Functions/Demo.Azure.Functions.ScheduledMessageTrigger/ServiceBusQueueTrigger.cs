using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Demo.Function
{
    public class ServiceBusQueueTrigger
    {
        [FunctionName("ServiceBusQueueTrigger")]
        public void Run([ServiceBusTrigger("sb-que-demo-dev", Connection = "sbDEMODEV_SERVICEBUS")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("InvoiceSchedule");

            ScheduledMessage msg = JsonConvert.DeserializeObject<ScheduledMessage>(myQueueItem);
            // Create a new entity
            var entity = new MyTableEntity
            {
                PartitionKey = msg.InterfaceName,
                RowKey = msg.Id,
                Status = msg.Event+'d'
            };
            TableOperation insertOperation = TableOperation.InsertOrMerge(entity);
            table.ExecuteAsync(insertOperation);
            log.LogInformation("Entity inserted into table");
        }
    }
    public class MyTableEntity : TableEntity
    {
        public string Status { get; set; }
    }
    public class ScheduledMessage
    {
        public string Id { get; set; }
        public string InterfaceName { get; set; } 
        public string Event { get; set; } 
    }
}
