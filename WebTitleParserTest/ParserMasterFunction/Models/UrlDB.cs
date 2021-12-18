using System;
using Newtonsoft.Json;

namespace ParserMasterFunction
{
    public class UrlDB
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("url")]
        public string URL { get; set; }
        [JsonProperty("domain")]
        public string Domain { get; set; }
    }
}
