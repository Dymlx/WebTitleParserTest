using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace TestTitleCrawlerClient.Pages
{
    public class TitleResponce
    {
        [JsonProperty("titles")]
        public string[] Titles { get; set; }
        [JsonProperty("urls")]
        public string[] Urls { get; set; }
        [JsonProperty("domain")]
        public string Domain { get; set; }
    }

    public class ResultsModel : PageModel
    {
        private readonly ILogger<ResultsModel> _logger;

        public List<TitleResponce> TitlesResponse { get; set; }

        public ResultsModel(ILogger<ResultsModel> logger)
        {
            _logger = logger;
        }
        public async Task OnGet()
        {
            using HttpClient client = new HttpClient();
            var response = await client.GetAsync("https://testtitlecrawler.azurewebsites.net/api/Crawled");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var pageContents = await response.Content.ReadAsStringAsync();
                var titles = JsonConvert.DeserializeObject<List<TitleResponce>>(pageContents);
                TitlesResponse = titles;
            }
        }
    }
}
