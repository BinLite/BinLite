using Newtonsoft.Json;

namespace BinLiteServer
{
    [Serializable]
    public class History
    {
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }
        [JsonProperty("type")]
        public HistoryType Type { get; set; }
        [JsonProperty("source")]
        public string? Source { get; set; }
        [JsonProperty("realm")]
        public string? Realm { get; set; }
        [JsonProperty("entity")]
        public string? Entity { get; set; }
        [JsonProperty("field")]
        public string? Field { get; set; }
        [JsonProperty("from")]
        public string? From { get; set; }
        [JsonProperty("to")]
        public string? To { get; set; }
        [JsonProperty("note")]
        public string? Note { get; set; }
    }
}
