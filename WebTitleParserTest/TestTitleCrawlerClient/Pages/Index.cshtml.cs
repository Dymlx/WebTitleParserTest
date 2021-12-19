using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TestTitleCrawlerClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public string Urls { get; set; }
        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public async Task OnPost()
        {
            var urls = Urls?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            if (urls != null && urls.Length > 0)
            {
                var json = JsonConvert.SerializeObject(urls);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                using HttpClient client = new HttpClient();
                var response = await client.PostAsync("https://testtitlecrawler.azurewebsites.net/api/ToCrawl", content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Urls = string.Empty;
                }
                ViewData["SubmitStatus"] = response.StatusCode;
            }
        }
    }
}
