using Newtonsoft.Json;

namespace ParserMasterFunction
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
}
