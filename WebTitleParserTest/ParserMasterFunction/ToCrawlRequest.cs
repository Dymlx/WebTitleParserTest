using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;

namespace ParserMasterFunction
{
    public static class ToCrawlRequest
    {
        [FunctionName("ToCrawlRequest")]
        public static async Task<IActionResult> RunToCrawl(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ToCrawl")] HttpRequest req,
            [Queue("urltocrawl", Connection = "QUEUE_CONNECTION_STRING")] IAsyncCollector<UrlDB> queueCollector,
            [CosmosDB("CrawlerDB", "urls", Id = "id", ConnectionStringSetting = "COSMOS_DB_CONNECTION_STRING")] DocumentClient client,
            [CosmosDB("CrawlerDB", "urls", Id = "id", ConnectionStringSetting = "COSMOS_DB_CONNECTION_STRING")] IAsyncCollector<UrlDB> dbCollector,
            ILogger log)
        {
            log.LogInformation("C# HTTP CrawlerRequest trigger function processing POST request.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var urlsArray = ParseRequestBody(requestBody);

            urlsArray = await FilterUrls(client, urlsArray);

            foreach (var url in urlsArray)
            {
                await dbCollector.AddAsync(url);
                await queueCollector.AddAsync(url);
            }

            return new OkResult();
        }

        private static async Task<UrlDB[]> FilterUrls(DocumentClient client, UrlDB[] urlsArray)
        {
            var results = new List<UrlDB>();
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("CrawlerDB", "urls");

            foreach (var url in urlsArray)
            {
                IDocumentQuery<UrlDB> query = client.CreateDocumentQuery<UrlDB>(collectionUri)
                    .Where(u => u.Domain == url.Domain && u.URL == url.URL)
                    .AsDocumentQuery();

                bool found = false;
                while (query.HasMoreResults)
                {
                    foreach (UrlDB result in await query.ExecuteNextAsync())
                    {
                        found = true;
                        break;
                    }
                    if (found) 
                        break;
                }
                if (!found)
                    results.Add(url);
            }
            return results.ToArray();
        }

        public static UrlDB[] ParseRequestBody(string requestBody)
        {
            var urls = JsonConvert.DeserializeObject<List<string>>(requestBody);
            var list = new List<UrlDB>();
            foreach (var url in urls)
            {
                var urlObj = new Uri(url);
                list.Add(new UrlDB { Id = Guid.NewGuid(), Domain = urlObj.Host.ToLower(), URL = url.ToLower() });
            }
            return list.ToArray();
        }


    }
}
