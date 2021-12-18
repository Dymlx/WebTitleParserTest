using System;
using Newtonsoft.Json;

namespace ParserMasterFunction
{
    public class TitleDB
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("domain")]
        public string Domain { get; set; }
    }
}
