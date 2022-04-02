namespace BinLiteServer
{
    [Serializable]
    public class History
    {
        public string ID { get; set; }
        public DateTime Timestamp { get; set; }
        public HistoryType Type { get; set; }
        public string? Source { get; set; }
        public string? Realm { get; set; }
        public string? Entity { get; set; }
        public string? Field { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public string? Note { get; set; }
    }
}
