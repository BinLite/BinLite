using System.Text.RegularExpressions;

using BC = BCrypt.Net;

namespace BinLiteServer
{
    [DataConnector]
    public static class Connector_User
    {
        private static Dictionary<string, string> LoginCache = new(); 

        public static void ConfirmTables()
        {
            MySqlManager.Execute(
@"CREATE TABLE IF NOT EXISTS `@p0users` (
  `id` varchar(24) NOT NULL,
  `email` varchar(128) DEFAULT NULL,
  `username` varchar(45) DEFAULT NULL,
  `enabled` bit(1) DEFAULT NULL,
  `salt` varchar(256) DEFAULT NULL,
  `password` varchar(256) DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `ID_UNIQUE` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
");
            LoginCache = new();
        }

        private static readonly Random random = new();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static User CreateUser(string username, string email, string caller)
        {
            if (caller is not null && !GetById(caller).ServerAdmin) { return null!; }

            var user = new User()
            {
                Username = Regex.Replace(username.ToLower().Trim(), @"\s+", "-"),
                Email = Regex.Replace(email.ToLower().Trim(), @"\s+", "-"),
                Enabled = true,
            };

            user.Username = user.Username.ToLower().Trim();
            var exists =
                MySqlManager.Read("SELECT COUNT(*) AS count FROM @p0users WHERE username=@p1 OR email=@p2;",
                user.Username, user.Email)
                [0]["count"].ToString() != "0";
            if (exists) { return null!; }

            user.ID = Snowflake.Generate().ToString();
            user.Salt = BC.BCrypt.GenerateSalt(12);
            var pw = RandomString(20);
            user.Password = BC.BCrypt.HashPassword(pw, user.Salt, true, BC.HashType.SHA512);

            MySqlManager.Execute("INSERT INTO @p0users VALUES (@p1, @p2, @p3, @p4, @p5, @p6);",
                user.ID, user.Email, user.Username, user.Enabled, user.Salt, user.Password);

            EmailManager.Send(user, 
                $"Acount Created", "Hello. An account has been created for you.\n\n" +
                $"Username: {user.Username}\nPassword: {pw}");
            return user;
        }

        public static User Basic(string username, string password)
        {
            username = username.ToLower().Trim();
            var r = MySqlManager.Read("SELECT * FROM @p0users WHERE username=@p1 AND enabled=true LIMIT 1;", username);
            if (r.Count == 0) { return null!; }
            var u = new User()
            {
                ID = r[0]["id"].ToString()!,
                Username = username,
                Email = r[0]["email"].ToString()!,
                Salt = r[0]["salt"].ToString()!,
                Password = r[0]["password"].ToString()!,
                Enabled = r[0]["enabled"].ToString()! == "1",
            };

            if (LoginCache.ContainsKey(username) && LoginCache[username] == password) { return u; }
            else if (LoginCache.ContainsKey(username)) { return null!; }

            var hash2 = BC.BCrypt.HashPassword(password, u.Salt, true, BC.HashType.SHA512);
            if (hash2 != u.Password) { return null!; }

            lock (LoginCache)
            {
                try
                {
                    LoginCache.Add(username, password);
                } 
                catch
                {
                    Logger.Warn("Login cache race condition issue.");
                }
            }

            return u;
        }

        public static List<User> GetAll()
        {
            var r = MySqlManager.Read("SELECT * FROM @p0users;");
            return r.Select(d =>
            {
                return new User()
                {
                    ID = d["id"].ToString()!,
                    Username = d["username"].ToString()!,
                    Email = d["email"].ToString()!,
                    Salt = d["salt"].ToString()!,
                    Password = d["password"].ToString()!,
                    Enabled = d["enabled"].ToString()! == "1",
                };
            }).ToList();
        }

        public static User GetById(string id)
        {
            var r = MySqlManager.Read("SELECT * FROM @p0users WHERE id=@p1;", id);
            if (r.Count <= 0) { return null!; }
            var d = r[0];
            return new User()
            {
                ID = d["id"].ToString()!,
                Username = d["username"].ToString()!,
                Email = d["email"].ToString()!,
                Salt = d["salt"].ToString()!,
                Password = d["password"].ToString()!,
                Enabled = d["enabled"].ToString()! == "1",
            };
        }

        public static bool ChangePassword(string caller, string oldPassword, string newPassword)
        {
            var u = GetById(caller);
            if (u is null) { return false; }

            if (Basic(u.Username, oldPassword) is null) { return false; }

            u.Salt = BC.BCrypt.GenerateSalt(12);
            u.Password = BC.BCrypt.HashPassword(newPassword, u.Salt, true, BC.HashType.SHA512);
            LoginCache = new();
            return MySqlManager.Execute("UPDATE @p0users SET salt=@p1,password=@p2 WHERE id=@p3", u.Salt, u.Password, u.ID) > 0;
        }

        public static bool ChangeEmail(string user, string newEmail, string caller)
        {
            var callerUser = GetById(caller);
            if (user != caller && !callerUser.ServerAdmin) { return false; }
            return MySqlManager.Execute("UPDATE @p0users SET email=@p1 WHERE id=@p2", newEmail, user) > 0;
        }

        public static bool ResetPassword(string toChange, string caller)
        {
            var callerUser = GetById(caller);
            if (!callerUser.ServerAdmin) { return false; }

            var user = GetById(toChange);
            if (user is null) { return false; }

            user.Salt = BC.BCrypt.GenerateSalt(12);
            var pw = RandomString(20);
            user.Password = BC.BCrypt.HashPassword(pw, user.Salt, true, BC.HashType.SHA512);

            EmailManager.Send(user,
                $"Acount Created", $"Hello, {user.Username}. Your password has been reset by a server admin.\n\n" +
                $"Username: {user.Username}\nPassword: {pw}");

            LoginCache = new();
            return MySqlManager.Execute("UPDATE @p0users SET salt=@p1,password=@p2 WHERE id=@p3", user.Salt, user.Password, user.ID) > 0;
        }

        public static bool SetEnabled(string toChange, bool status, string caller)
        {
            var callerUser = GetById(caller);
            if (!callerUser.ServerAdmin) { return false; }

            var user = GetById(toChange);
            if (user is null) { return false; }

            return MySqlManager.Execute("UPDATE @p0users SET enabled=@p1 WHERE id=@p2", status, toChange) > 0;
        }
    }
}
