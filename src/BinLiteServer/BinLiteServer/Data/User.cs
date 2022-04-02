using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BinLiteServer
{
    [Serializable]
    public class User
    {
        [JsonProperty("ID")]
        public string ID { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("username")]
        public string Username { get; set; }
        [JsonProperty("enabled")]
        public bool Enabled { get; set; }
        [JsonProperty("salt")]
        public string Salt { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("serverAdmin")]
        public bool ServerAdmin => Program.Admins.Contains(Username);
        [JsonProperty("lastLogin")]
        public DateTime LastLogin { get; set; }

        public JObject ToFriendly() => new()
        {
            ["id"] = ID,
            ["email"] = Email,
            ["username"] = Username,
            ["serverAdmin"] = ServerAdmin
        };
    }
}
