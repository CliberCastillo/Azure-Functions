using Empleado.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Employes
{
    public static class EmployeTriggerCosmosDB
    {
        [FunctionName("EmployeTriggerCosmosDB")]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: "School",
                collectionName: "Employe",
                ConnectionStringSetting = "CosmosDBConnection",
                LeaseCollectionName = "leases", CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            [Queue("Employe",Connection = "AzureWebJobsStorage")] IAsyncCollector<Employe> orderQueue,
            [Blob("employe/{rand-guid}.json")] TextWriter outputblob,
            ILogger log)
        {
            if (input.Count > 0)
            {
                foreach (var item in input)
                {
                    outputblob.WriteLine(item);
                    await orderQueue.AddAsync(JsonConvert.DeserializeObject<Employe>(item.ToString()));
                }
            }
        }
    }
}
