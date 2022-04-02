using Newtonsoft.Json;

namespace BinLiteServer
{
    [Serializable]
    public class RealmPermission
    {
        [JsonProperty("realm")]
        public Realm Realm;
        [JsonProperty("permission")]
        public Permissions Permissions;
    }
}
