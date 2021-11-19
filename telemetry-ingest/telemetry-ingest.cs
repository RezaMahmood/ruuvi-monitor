using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Identity;
using Microsoft.Azure.Cosmos;

namespace rezamahmood.telemetry
{
    public static class telemetry_ingest
    {
        [FunctionName("telemetry_ingest")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("telemetry-ingest request received");

            try
            {
                var cosmosDatabase = System.Environment.GetEnvironmentVariable("CosmosDatabaseId");
                var cosmosContainer = System.Environment.GetEnvironmentVariable("CosmosContainerId");
                var cosmosClient = new CosmosClient(System.Environment.GetEnvironmentVariable("CosmosAccountUri"), new DefaultAzureCredential());
                var container = cosmosClient.GetContainer(cosmosDatabase, cosmosContainer);

                string requestBody = string.Empty;
                using (StreamReader streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }

                dynamic data = JsonConvert.DeserializeObject(requestBody);

                await container.CreateItemAsync(data, new PartitionKey(data.sensorname));

            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw;
            }

            return new AcceptedResult();
        }
    }
}
