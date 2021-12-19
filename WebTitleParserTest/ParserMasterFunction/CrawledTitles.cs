using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using System;
using Microsoft.Azure.Documents.Linq;

namespace ParserMasterFunction
{
    public static class CrawledTitles
    {
        [FunctionName("CrawledTitles")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Crawled")] HttpRequest req,
            [CosmosDB("CrawlerDB", "titles", Id = "id", ConnectionStringSetting = "COSMOS_DB_CONNECTION_STRING")] DocumentClient client,
            ILogger log)
        {
            log.LogInformation("C# HTTP CrawledUrls trigger function processing GET request.");

            var titles = await GetAllTitles(client);

            var results = CreateAggregatedResults(titles);

            log.LogInformation("C# HTTP CrawledUrls trigger function processed GET request.");
            return new OkObjectResult(results);
        }

        private static TitleResponce[] CreateAggregatedResults(IEnumerable<TitleDB> urls)
        {
            var dictionary = new Dictionary<string, (List<string> Titles, List<string> Urls)>();
            foreach(var url in urls)
            {
                if (!dictionary.ContainsKey(url.Domain)) 
                    dictionary.Add(url.Domain, (new List<string>(), new List<string>()));
                dictionary[url.Domain].Titles.Add(url.Title);
                dictionary[url.Domain].Urls.Add(url.Url);
            }
            return dictionary.Select(kvp => new TitleResponce
            {
                Domain = kvp.Key,
                Titles = kvp.Value.Titles.ToArray(), 
                Urls = kvp.Value.Urls.ToArray() 
            }).ToArray();
        }

        private static async Task<TitleDB[]> GetAllTitles(DocumentClient client)
        {
            var results = new List<TitleDB>();
            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("CrawlerDB", "titles");

            IDocumentQuery<TitleDB> query = client.CreateDocumentQuery<TitleDB>(collectionUri)
                .AsDocumentQuery();

            while (query.HasMoreResults)
            {
                foreach (TitleDB result in await query.ExecuteNextAsync())
                {
                    results.Add(result);
                }
            }

            return results.ToArray();
        }
    }
}
