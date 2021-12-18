using System;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ParserMasterFunction
{
    public static class TitleCrawler
    {
        [FunctionName("TitleCrawler")]
        public static async Task Run(
            [QueueTrigger("urltocrawl", Connection = "QUEUE_CONNECTION_STRING")] UrlDB urlToCrawl,
            [CosmosDB("CrawlerDB", "titles", Id = "id", ConnectionStringSetting = "COSMOS_DB_CONNECTION_STRING")] IAsyncCollector<TitleDB> titlesDB,
            ILogger log)
        {
            log.LogInformation($"C# Queue TitleCrawler trigger function processing {urlToCrawl.URL} request.");
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(urlToCrawl.URL);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var pageContents = await response.Content.ReadAsStringAsync();
                var title = ExtractTitle(pageContents);
                var titleDB = new TitleDB
                {
                    Id = Guid.NewGuid(),
                    Url = urlToCrawl.URL,
                    Domain = urlToCrawl.Domain,
                    Title = title
                };
                await titlesDB.AddAsync(titleDB);
                log.LogInformation($"C# Queue TitleCrawler trigger function processed {urlToCrawl.URL} request. Title: {title}");
            }
            else
            {
                log.LogInformation($"C# Queue TitleCrawler trigger function processed {urlToCrawl.URL} request. Status: {response.StatusCode}");
            }
        }

        private static string ExtractTitle(string pageContents)
        {
            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(pageContents);
            var titleNode = pageDocument.DocumentNode.SelectSingleNode("/html/head/title");
            if (titleNode != null)
            {
                return titleNode.InnerText;
            }
            return null;
        }
    }
}
