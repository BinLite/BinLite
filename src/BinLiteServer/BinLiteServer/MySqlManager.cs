using MySql.Data.MySqlClient;

namespace BinLiteServer
{
    public static class MySqlManager
    {
        public static string TablePrefix { get; private set; }
        public static string String { get; private set; }
        public static int MaxPool { get; private set; }

        public static void Init()
        {
            Logger.Debug("Initializing MySqlManager.");
            var host = Configuration.Get<string>("mysql.host");
            var port = Configuration.Get<int>("mysql.port");
            var database = Configuration.Get<string>("mysql.database");
            var user = Configuration.Get<string>("mysql.user");
            var password = Configuration.Get<string>("mysql.password");
            TablePrefix = Configuration.Get<string>("mysql.tablePrefix");
            MaxPool = Configuration.Get<int>("mysql.connectionPoolSize");

            String = $"server={host};user={user};database={database};port={port};password={password};pooling=true";

            Logger.Debug("Establishing MySql connection pool...");
            var start = DateTime.Now;
            var connections = new List<MySqlConnection>();
            for (var i = 0; i < MaxPool; i++)
            {
                try
                {
                    connections.Add(new MySqlConnection(String));
                    connections.Last().Open();
                }
                catch (Exception ex)
                {
                    Logger.Fatal("Failed to establish a database connection. " +
                        "This could either mean it couldn't be reached, or another server instance is using all available connections. " +
                        "Please see the following error.\n" + ex);
                    throw;
                }
            }
            connections.ForEach(c => c.Close());
            var time = (DateTime.Now - start).TotalSeconds;
            Logger.Info($"Established MySql connection pool of size {MaxPool}. ({time:0.0} seconds)");
        }

        /// <summary>
        /// Be sure to Close or Dispose this connection when done! That returns it to the pool.
        /// </summary>
        private static MySqlCommand Run(string sql)
        {
            var conn = new MySqlConnection(String);
            conn.Open();
            var cmd = new MySqlCommand(sql, conn);
            return cmd;
        }

        private static void HandleEnd(MySqlCommand cmd, MySqlDataReader r = null!)
        {
            if (r is not null) { r.Close(); r.Dispose(); }

            cmd.Connection.Close();
            cmd.Connection.Dispose();
            cmd.Dispose();
        }

        public static int Execute(string sql, params object?[] parameters)
        {
            sql = sql.Replace("@p0", TablePrefix);
            using var conn = Run(sql);
            var i = 0;
            foreach (var p in parameters)
            {
                i++;
                conn.Parameters.AddWithValue("@p" + i, p);
            }
             var rows = conn.ExecuteNonQuery();
            HandleEnd(conn);
            return rows;
        }

        public static List<Dictionary<string, object>> Read(string sql, params object[] parameters)
        {
            sql = sql.Replace("@p0", TablePrefix);
            sql = sql.EndsWith(";") ? sql : sql + ";";
            using var conn = Run(sql);
            var i = 0;
            foreach (var p in parameters)
            {
                i++;
                conn.Parameters.AddWithValue("@p" + i, p);
            }
            var reader = conn.ExecuteReader();
            var toReturn = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (var j = 0; j < reader.FieldCount; j++)
                {
                    row.Add(reader.GetName(j), reader[j]);
                }
                toReturn.Add(row);
            }
            HandleEnd(conn, reader);
            return toReturn!;
        }
    }
}
