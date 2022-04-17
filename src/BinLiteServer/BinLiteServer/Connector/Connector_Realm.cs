using Newtonsoft.Json.Linq;

namespace BinLiteServer
{
    [DataConnector]
    public static class Connector_Realm
    {
        public static void ConfirmTables()
        {
            MySqlManager.Execute(
@"CREATE TABLE IF NOT EXISTS `@p0realms` (
  `id` varchar(24) NOT NULL,
  `owner` varchar(24) DEFAULT NULL,
  `name` varchar(128) DEFAULT NULL,
  `enabled` bit(1) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
");
            MySqlManager.Execute(
@"CREATE TABLE IF NOT EXISTS `@p0realm_user` (
  `realm` varchar(24) DEFAULT NULL,
  `user` varchar(24) DEFAULT NULL,
  `permission` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
");
        }

        public static List<RealmPermission> GetAll(string caller)
        {
            var serverAdmin = Connector_User.GetById(caller).ServerAdmin;

            var r = MySqlManager.Read("SELECT * FROM (SELECT rm.*, IF(@p2 OR rm.owner=@p1, 3, ru.permission) AS permission FROM @p0realms rm LEFT JOIN @p0realm_user ru ON (ru.realm=rm.id AND ru.user=@p1)) t WHERE t.permission > 0;", caller, serverAdmin);
            return r.Select(d =>
            {
                return new RealmPermission()
                {
                    Realm = new Realm()
                    {
                        ID = d["id"].ToString()!,
                        Owner = d["owner"].ToString()!,
                        Name = d["name"].ToString()!,
                        Enabled = d["enabled"].ToString()! == "1",
                    },
                    Permissions = serverAdmin || (d["owner"].ToString()! == caller) ? Permissions.Admin : (Permissions)int.Parse(d["permission"].ToString()!)
                };
            }).ToList();
        }

        public static Realm GetByID(string id, string caller)
        {
            var serverAdmin = Connector_User.GetById(caller).ServerAdmin;

            var r = MySqlManager.Read("SELECT rm.* FROM @p0realms rm LEFT JOIN @p0realm_user ru ON " +
                "(ru.realm=rm.id AND ru.user=@p1) WHERE (@p2 OR ru.permission > 0 OR rm.owner=@p1) AND rm.id=@p3;", caller, serverAdmin, id);
            if (r.Count == 0) { return null!; }
            return new Realm()
            {
                ID = r[0]["id"].ToString()!,
                Owner = r[0]["owner"].ToString()!,
                Name = r[0]["name"].ToString()!,
                Enabled = r[0]["enabled"].ToString()! == "1",
            };
        }

        public static Realm Create(Realm r, string caller)
        {
            if (!Connector_User.GetById(caller).ServerAdmin) { return null!; }

            var exists = MySqlManager.Read("SELECT COUNT(*) AS COUNT FROM @p0realms WHERE id=@p1;", r.ID)[0]["COUNT"].ToString() != "0";
            if (exists) { return null!; }

            MySqlManager.Execute("INSERT INTO @p0realms VALUES (@p1, @p2, @p3, @p4);", r.ID, r.Owner, r.Name, r.Enabled);

            var owner = Connector_User.GetAll().First(u => u.ID == r.Owner);
            EmailManager.Send(owner, "Realm Created", $"Hello, {owner.Username}.\n " +
                $"This message is to inform you that a new realm, {r.Name}, has been created with your account as owner.\nThank you.");

            return r;
        }

        public static Realm Update(Realm realm, string caller)
        {
            if (GetPermission(caller, realm.ID) < Permissions.Admin) { return null!; }
            var old = GetByID(realm.ID, caller);
            var callerUser = Connector_User.GetById(caller);
            if (old.Owner != realm.Owner && (old.Owner != caller && !callerUser.ServerAdmin))
            {
                return null!;
            } 
            else if (old.Owner != realm.Owner)
            {
                MySqlManager.Execute("DELETE FROM @p0realm_user WHERE realm=@p1 AND user=@p2", realm.ID, old.Owner);
                MySqlManager.Execute("INSERT INTO @p0realm_user VALUES (@p1, @p2, 3)", realm.ID, old.Owner);
            }

            var count = MySqlManager.Execute("UPDATE @p0realms SET name=@p1,owner=@p2,enabled=@p3 WHERE id=@p4",
                realm.Name, realm.Owner, realm.Enabled, realm.ID);
            return count > 0 ? GetByID(realm.ID, caller) : null!;
        }

        public static Permissions GetPermission(string user, string realm)
        {
            var admin = Connector_User.GetById(user).ServerAdmin;

            var r = MySqlManager.Read("SELECT IF(@p2 OR rm.owner=@p1, 3, ru.permission) AS permission FROM @p0realms rm LEFT JOIN @p0realm_user ru ON " +
                "(ru.realm=rm.id AND ru.user=@p1) WHERE rm.id=@p3;", user, admin, realm);
            if (r.Count <= 0) { return Permissions.None; }
            return (Permissions)int.Parse(r[0]["permission"].ToString()!);
        }

        public static Permissions? SetPermission(string user, string realm, Permissions p, string caller)
        {
            if (GetPermission(caller, realm) != Permissions.Admin) { return null!; }

            MySqlManager.Execute("DELETE FROM @p0realm_user WHERE user=@p1 AND realm=@p2;", user, realm);
            MySqlManager.Execute("INSERT INTO @p0realm_user VALUES (@p1, @p2, @p3)", realm, user, p);
            return GetPermission(user, realm);
        }

        public static JArray GetAllPermissions(string realm)
        {
            var perms = Connector_User.GetAll().Select(u => (u.ID, GetPermission(u.ID, realm))).ToList();
            var toReturn = new JArray();
            perms.ForEach(p => toReturn.Add(new JObject()
            {
                ["id"] = p.ID,
                ["permission"] = (int)p.Item2,
            }));
            return toReturn;
        }
    }
}
