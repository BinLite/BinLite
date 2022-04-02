using Newtonsoft.Json;

namespace BinLiteServer
{
    [Serializable]
    public class Realm
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("owner")]
        public string Owner { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
