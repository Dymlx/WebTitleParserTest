using Newtonsoft.Json;

namespace ParserMasterFunction
{
    public class UrlResponce
    {
        [JsonProperty("urls")]
        public string[] URLs { get; set; }
        [JsonProperty("domain")]
        public string Domain { get; set; }
    }
}
