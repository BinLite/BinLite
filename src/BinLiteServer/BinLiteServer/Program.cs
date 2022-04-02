using MySql.Data.MySqlClient;

using Newtonsoft.Json.Linq;

namespace BinLiteServer
{
    public static class Program
    {
        static DateTime start;
        public static List<string> Admins { get; private set; }

        public static void Main(string[] args)
        {
            start = DateTime.Now;
            if (args is null || args.Length < 1 || !Directory.Exists(args[0]))
            {
                Console.WriteLine("Invalid runtime directory.");
                throw new ArgumentException("Invalid runtime directory.");
            }

            Directory.SetCurrentDirectory(args[0]);
            Configuration.Init();

            Logger.Init();
            Logger.Debug("Runtime directory is " + args[0]);
            Logger.Info($"Hello, world! Starting up...");

            MySqlManager.Init();
            DataConnector.Call("ConfirmTables");
            APIManager.Init();

            Admins = new();
            var admins = Configuration.Get<JArray>("admins");
            foreach (JObject admin in admins)
            {
                Admins.Add(admin["username"]!.Value<string>()!);
                var user = Connector_User.CreateUser(admin["username"]!.Value<string>()!, admin["email"]!.Value<string>()!, null!);
                if (user is not null)
                {
                    Logger.Warn("Created new admin account: " + admin.ToString(Newtonsoft.Json.Formatting.None));
                }
            }

            var time = (DateTime.Now - start).TotalSeconds;
            Logger.Info($"Server online! ({time:0.0} seconds) ({Snowflake.Generate()})");

            while (true)
            {
                var input = Console.ReadLine();
                if (input == "exit")
                {
                    var startExit = DateTime.Now;
                    Logger.Info("Exiting...");
                    Logger.Info("Stopping WebSocket server...");
                    Logger.Info("Stopping MySql connection pool...");
                    MySqlConnection.ClearAllPools();
                    var timeExit = (DateTime.Now - startExit).TotalSeconds;
                    var runtime = (DateTime.Now - start).TotalSeconds;
                    Logger.Info($"Goodbye! ({timeExit:0.0} seconds) (uptime {runtime:0.0} seconds)");
                    while (!Logger.LogQueue.IsEmpty || Logger.IsLogging) { Thread.Sleep(1); }
                    Environment.Exit(0);
                }
                else if (input == "wipe")
                {
                    Logger.Warn("Are you sure you would like to wipe ALL data, including all items, users, realms, history, etc? (Y to continue)");
                    var r = Console.ReadLine()!.ToUpper().Trim();
                    if (r != "Y") { Console.WriteLine("Data wipe cancelled."); continue; }
                    Logger.Warn("Are you ABSOLUTELY SURE? This will delete literally everything. (SURE to continue)");
                    r = Console.ReadLine()!.ToUpper().Trim();
                    if (r != "SURE") { Console.WriteLine("Data wipe cancelled."); continue; }
                    Logger.Warn("Wiping all data....");
                    try
                    {
                        MySqlManager.Execute("DROP TABLE @p0items");
                        Logger.Warn("Dropped items");
                        MySqlManager.Execute("DROP TABLE @p0users");
                        Logger.Warn("Dropped users");
                        MySqlManager.Execute("DROP TABLE @p0realm_user");
                        Logger.Warn("Dropped realm_user");
                        MySqlManager.Execute("DROP TABLE @p0realms");
                        Logger.Warn("Dropped realms");
                        //MySqlManager.Execute("DROP TABLE @p0history");
                        //Logger.Warn("Dropped history");
                    }
                    catch (Exception e)
                    {
                        Logger.Fatal("Failed mid-wipe! Please retry to avoid a corrupted state!\n\n" + e);
                        continue;
                    }
                    Logger.Warn("All data wiped. Exiting...");
                    Environment.Exit(0);
                }
            }
        }
    }
}