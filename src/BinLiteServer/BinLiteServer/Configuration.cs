using Newtonsoft.Json.Linq;

namespace BinLiteServer
{
    public static class Configuration
    {
        public static JObject Config { get; private set; } = JObject.Parse("{ }");

        public static void Init()
        {
            if (!File.Exists("config.json"))
            {
                Console.WriteLine("Failed to find config.json in rutime directory.");
                throw new ArgumentException("Failed to find config.json in rutime directory.");
            }

            try
            {
                Config = JObject.Parse(File.ReadAllText("config.json"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to parse config.json.");
                throw new ArgumentException("Failed to parse config.json.", ex);
            }
            Console.WriteLine("Config loaded.");
        }

        public static T Get<T>(string path)
        {
            var token = Config.SelectToken(path);
            if (token is null)
            {
                Console.WriteLine("Configuration item not found: " + path);
                return default!;
            }
            try
            {
                return token.Value<T>()!;
            }
            catch
            {
                Console.WriteLine("Failed to cast type (" + typeof(T).Name + "): " + path);
                return default!;
            }
        }
    }
}
