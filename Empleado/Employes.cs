using System;
using System.IO;
using System.Threading.Tasks;
using Empleado.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Empleado
{
    public static class Employes
    {
        [FunctionName("Employe")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "School",
                collectionName: "Employe",
                ConnectionStringSetting = "CosmosDBConnection")]
                IAsyncCollector<Employe> employes,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var employe = JsonConvert.DeserializeObject<Employe>(requestBody);
            await employes.AddAsync(new Employe
            {
                EmployeId = Guid.NewGuid().ToString(),
                Name = employe.Name,
                LastName = employe.LastName,    
                Age = employe.Age
            });
            log.LogInformation($"Name {employe.Name}");
            log.LogInformation($"LastName {employe.LastName}");
            log.LogInformation($"Age {employe.Age}");
            return new OkObjectResult(employe);
        }
    }
}
