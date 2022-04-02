using Newtonsoft.Json;

namespace BinLiteServer
{
    [Serializable]
    public class Item
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("realm")]
        public string Realm { get; set; }
        [JsonProperty("parent")]
        public string? Parent { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
