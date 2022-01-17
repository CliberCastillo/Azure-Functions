using Empleado.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

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
            [Queue("Employe",Connection = "AzureWebJobsStorage")] IAsyncCollector<Employe> enployeQueue,
            [Blob("employe/{rand-guid}.json")] TextWriter outputblob,
            [TwilioSms(AccountSidSetting = "TwilioAccountSid",AuthTokenSetting = "TwilioAuthToken")] IAsyncCollector<CreateMessageOptions> messageCollector,
            ILogger log)
        {
            Employe empleado = new Employe();
            if (input.Count > 0)
            {
                foreach (var item in input)
                {
                    outputblob.WriteLine(item);
                    empleado = JsonConvert.DeserializeObject<Employe>(item.ToString());
                    await enployeQueue.AddAsync(empleado);
                }
            }
            log.LogInformation($"SendSmsTimer executed at: {DateTime.Now}");

            string toPhoneNumber = Environment.GetEnvironmentVariable("ToPhoneNumber", EnvironmentVariableTarget.Process);
            string fromPhoneNumber = Environment.GetEnvironmentVariable("FromPhoneNumber", EnvironmentVariableTarget.Process);
            var message = new CreateMessageOptions(new PhoneNumber(toPhoneNumber))
            {
                From = new PhoneNumber(fromPhoneNumber),
                Body = $"Hola su nombre registrado es {empleado.Name} {empleado.LastName} edad {empleado.Age} - Probando azure functions"
            };
            await messageCollector.AddAsync(message);
            log.LogInformation($"send message: {message}");
        }
    }
}
