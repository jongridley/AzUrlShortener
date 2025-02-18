﻿/*
```c#
Input:
    {
         // [Required]
        "Metadata": {

         // [Required]
        "ShipmentID": "123456"

        }
    }
Output:
    {
        "Url": "https://docs.microsoft.com/en-ca/azure/azure-functions/functions-create-your-first-function-visual-studio",
        "Title": "My Title",
        "ShortUrl": null,
        "Clicks": 0,
        "IsArchived": true,
        "PartitionKey": "a",
        "RowKey": "azFunc2",
        "Timestamp": "2020-07-23T06:22:33.852218-04:00",
        "ETag": "W/\"datetime'2020-07-23T10%3A24%3A51.3440526Z'\""
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
    public static class UrlMetadataQuery
    {
        [FunctionName("UrlMetadataQuery")]
        public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger log,
        ExecutionContext context)
        {
            log.LogInformation($"C# HTTP trigger function processed this request: {req}");

            string userId = string.Empty;
            ShipmentMetadata input;
            ShortUrlEntity result;
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
                    input = JsonSerializer.Deserialize<ShipmentMetadata>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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

                result = stgHelper.GetShortUrlEntity(input);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An unexpected error was encountered.");
                return new BadRequestObjectResult(new
                {
                    message = ex.Message,
                    StatusCode = HttpStatusCode.BadRequest
                });
            }

            return new OkObjectResult(result);
        }
    }
}
