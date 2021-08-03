/*
```c#
Input:
    {
         // [Required]
        "Days": "60",

        // [Optional] all other properties
    }
Output:
    {
        true
    }

*/

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using Cloud5mins.domain;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.Json;

namespace Cloud5mins.Function
{
    public static class UrlDelete
    {
        [FunctionName("UrlDelete")]
        public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger log,
        ExecutionContext context)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            string userId = string.Empty;
            DeleteRequest input;
            bool result;
            try
            {

                // Validation of the inputs
                if (req == null)
                {
                    return new BadRequestObjectResult(new { StatusCode = HttpStatusCode.NotFound });
                }

                using (var reader = new StreamReader(req.Body))
                {
                    var body = reader.ReadToEnd();
                    input = JsonSerializer.Deserialize<DeleteRequest>(body, new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
                    if (input == null)
                    {
                        return new BadRequestObjectResult(new { StatusCode = HttpStatusCode.NotFound });
                    }
                }

                var config = new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                StorageTableHelper stgHelper = new StorageTableHelper(config["UlsDataStorage"]);

                result = await stgHelper.DeleteExpired(input.Days);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An unexpected error was encountered.");
                return new BadRequestObjectResult(new {
                                                        message = ex.Message,
                                                        StatusCode = HttpStatusCode.BadRequest
                                                    });
                }

            return new OkObjectResult(result);
        }
    }
}
