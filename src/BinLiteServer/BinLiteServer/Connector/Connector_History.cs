using System;

namespace BinLiteServer
{
    [DataConnector]
    public static class Connector_History
    {
        public static void ConfirmTables()
        {
            MySqlManager.Execute(
@"CREATE TABLE IF NOT EXISTS `@p0history` (
  `id` varchar(24) NOT NULL,
  `timestamp` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `type` int(11) NOT NULL,
  `source` varchar(24) DEFAULT NULL,
  `realm` varchar(24) DEFAULT NULL,
  `entity` varchar(24) DEFAULT NULL,
  `field` varchar(24) DEFAULT NULL,
  `from` text DEFAULT NULL,
  `to` text DEFAULT NULL,
  `note` text DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
");
        }

        public static History Add(History h)
        {
            h.ID = Snowflake.Generate(2).ToString();
            h.Timestamp  = DateTime.Now;
            MySqlManager.Execute("INSERT INTO @p0history VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10)",
                h.ID, h.Timestamp, h.Type, h.Source, h.Realm, h.Entity, h.Field, h.From, h.To, h.Note);
            return h;
        }

        public static int GetSize(string caller, string realmFilter = null!, string sourceFilter = null!)
        {
            var callerUser = Connector_User.GetById(caller);
            if ((realmFilter is null || realmFilter == "") && !callerUser.ServerAdmin) { return new(); }
            if ((realmFilter is not null && realmFilter != "") && Connector_Realm.GetPermission(caller, realmFilter) < Permissions.Read) { return new(); }

            var q = "SELECT COUNT(*) AS count FROM @p0history";

            var p = new List<object>();
            var pI = 1;

            var conditions = new List<string>();

            if (sourceFilter is not null && sourceFilter != "") { conditions.Add($"source=@p{pI++}"); p.Add(sourceFilter); }
            if (realmFilter is not null && realmFilter != "") { conditions.Add($"realm=@p{pI++}"); p.Add(realmFilter); }

            var pageSize = 100;
            if (conditions.Count > 0)
            {
                q += " WHERE (" + string.Join(") AND (", conditions) + $")";
            }

            var rows = MySqlManager.Read(q, p.ToArray());
            return (int)Math.Ceiling(float.Parse(rows[0]["count"].ToString()!) / pageSize);
        }

        public static List<History> Get(int page, string caller, string realmFilter = null!, string sourceFilter = null!)
        {
            page--;
            var callerUser = Connector_User.GetById(caller);
            if ((realmFilter is null || realmFilter == "") && !callerUser.ServerAdmin) { return new(); }
            if ((realmFilter is not null && realmFilter != "") && Connector_Realm.GetPermission(caller, realmFilter) < Permissions.Read) { return new(); }

            var q = "SELECT * FROM @p0history";

            var p = new List<object>();
            var pI = 1;

            var conditions = new List<string>();

            if (sourceFilter is not null && sourceFilter != "") { conditions.Add($"source=@p{pI++}"); p.Add(sourceFilter); }
            if (realmFilter is not null && realmFilter != "") { conditions.Add($"realm=@p{pI++}"); p.Add(realmFilter); }

            var pageSize = 100;
            if (conditions.Count > 0)
            {
                q += " WHERE (" + string.Join(") AND (", conditions) + $")";
            }
            q += $" ORDER BY id DESC LIMIT {(page * pageSize)}, {(pageSize)};";

            var rows = MySqlManager.Read(q, p.ToArray());
            return rows.Select(d => new History()
            {
                ID = d["id"].ToString()!,
                Timestamp = (DateTime)d["timestamp"],
                Type = (HistoryType)int.Parse(d["type"].ToString()!),
                Source = d["source"] is DBNull ? null : d["source"].ToString()!,
                Realm = d["realm"] is DBNull ? null : d["realm"].ToString()!,
                Entity = d["entity"] is DBNull ? null : d["entity"].ToString()!,
                Field = d["field"] is DBNull ? null : d["field"].ToString()!,
                From = d["from"] is DBNull ? null : d["from"].ToString()!,
                To = d["to"] is DBNull ? null : d["to"].ToString()!,
                Note = d["note"] is DBNull ? null : d["note"].ToString()!,

            }).ToList();
        }
    }
}
