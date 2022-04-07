namespace BinLiteServer
{
    [DataConnector]
    public static class Connector_Item
    {
        public static void ConfirmTables()
        {
            MySqlManager.Execute(
@"CREATE TABLE IF NOT EXISTS `@p0items` (
  `id` varchar(24) NOT NULL,
  `realm` varchar(24) NOT NULL,
  `parent` varchar(24) DEFAULT NULL,
  `name` varchar(128) DEFAULT NULL,
  `description` mediumtext DEFAULT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `id_UNIQUE` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
");
        }

        public static List<Item> GetAll(string realm, string caller)
        {
            var perms = Connector_Realm.GetPermission(caller, realm);
            if (perms < Permissions.Read)
            {
                return null!;
            }

            var r = MySqlManager.Read("SELECT * FROM @p0items WHERE realm=@p1;", realm);
            return r.Select(d =>
            {
                return new Item()
                {
                    ID = d["id"].ToString()!,
                    Realm = realm,
                    Parent = d["parent"] is DBNull ? null : d["parent"].ToString()!,
                    Name = d["name"].ToString()!,
                    Description = d["description"] is DBNull ? null! : d["description"].ToString()!,
                };
            }).ToList();
        }

        public static Item GetByID(string id, string caller)
        {
            var r = MySqlManager.Read("SELECT * FROM @p0items WHERE id=@p1", id);
            if (r.Count == 0) { return null!; }
            var i = new Item()
            {
                ID = r[0]["id"].ToString()!,
                Realm = r[0]["realm"].ToString()!,
                Parent = r[0]["parent"] is DBNull ? null : r[0]["parent"].ToString()!,
                Name = r[0]["name"].ToString()!,
                Description = r[0]["description"] is DBNull ? null! : r[0]["description"].ToString()!,
            };
            if (Connector_Realm.GetPermission(caller, i.Realm) < Permissions.Read) { return null!; }
            return i;
        }
        
        private static bool Validate(Item item, string caller)
        {
            // Validate Realm
            var realm = Connector_Realm.GetByID(item.Realm, caller);
            if (realm is null) { return false!; }
            
            // Validate Name
            item.Name = item.Name.Trim();
            if (item.Name is null || item.Name.Length == 0) { return false; }

            // Validate Parent
            if (item.Parent is not null)
            {
                var parentItem = GetByID(item.Parent ?? default!, caller);
                if (parentItem is null) { return false; }
            }

            //Validate Description
            if (item.Description is not null && item.Description.Length > 4096) { return false; }

            return true;
        }

        public static Item Update(Item item, string caller)
        {
            if (!Validate(item, caller)) { return null!; }

            var oldItem = GetByID(item.ID, caller);
            if (oldItem is null) { return null!; }
            if (Connector_Realm.GetPermission(caller, oldItem.Realm) < Permissions.Write) { return null!; }
            if (Connector_Realm.GetPermission(caller, item.Realm) < Permissions.Write) { return null!; }

            var count = MySqlManager.Execute("UPDATE @p0items SET parent=@p1,name=@p2,description=@p3 WHERE id=@p4", 
                item.Parent ?? null!, item.Name, item.Description, item.ID);

            var h = new History()
            {
                Entity = item.ID,
                Source = caller,
                Realm = oldItem.Realm,
            };

            if (oldItem.Realm != item.Realm)
            {
                h.Type = HistoryType.ItemUpdate;
                h.From = oldItem.Realm;
                h.To = item.Realm;
                h.Field = "realm";
                Connector_History.Add(h);
            }
            
            if (oldItem.Name != item.Name)
            {
                h.Type = HistoryType.ItemUpdate;
                h.From = oldItem.Name;
                h.To = item.Name;
                h.Field = "name";
                Connector_History.Add(h);
            }

            if (oldItem.Description != item.Description)
            {
                h.Type = HistoryType.ItemUpdate;
                h.From = oldItem.Description;
                h.To = item.Description;
                h.Field = "realm";
                Connector_History.Add(h);
            }

            if (oldItem.Parent != item.Parent)
            {
                h.Type = HistoryType.ItemUpdate;
                h.From = oldItem.Parent;
                h.To = item.Parent;
                h.Field = "parent";
                Connector_History.Add(h);
            }

            return count > 0 ? item : null!;
        }

        public static bool Delete(string id, string caller)
        {
            var item = GetByID(id, caller);
            if (item == null) { return false; }
            if (Connector_Realm.GetPermission(caller, item.Realm) < Permissions.Write) { return false; }

            var children = MySqlManager.Read("SELECT it.id as 'id' FROM @p0items it WHERE it.parent=@p1;", item.ID).Select(d => d["id"].ToString()!).ToList();
            children.ForEach(c =>
            {
                Connector_History.Add(new History()
                {
                    Entity = c,
                    Source = caller,
                    Realm = item.Realm,
                    Type = HistoryType.ItemUpdate,
                    Field = "parent",
                    From = item.ID,
                    To = item.Parent,
                    Note = "Cause: Parent Deletion",
                });
            });

            Connector_History.Add(new History()
            {
                Entity = item.ID,
                Source = caller,
                Realm = item.Realm,
                Type = HistoryType.ItemDeleted,
                Field = null,
                From = item.ToJson(),
                To = null,
            });

            var deleted = MySqlManager.Execute("DELETE FROM @p0items WHERE id=@p1", id) > 0;
            MySqlManager.Execute("UPDATE @p0items it SET it.parent=@p1 WHERE it.parent=@p2;", item.Parent, item.ID);

            return deleted;
        }

        public static Item Add(Item item, string caller)
        {
            if (Connector_Realm.GetPermission(caller, item.Realm) < Permissions.Write) { return null!; }
            if (!ulong.TryParse(item.ID, out _)) { return null!; }


            if (!Validate(item, caller)) { return null!; }
            MySqlManager.Execute("INSERT INTO @p0items VALUES (@p1, @p2, @p3, @p4, @p5)",
                item.ID, item.Realm, item.Parent ?? null!, item.Name, item.Description);

            Connector_History.Add(new History()
            {
                Entity = item.ID,
                Source = caller,
                Realm = item.Realm,
                Type = HistoryType.ItemCreated,
                Field = null,
                From = null,
                To = item.ToJson(),
            });

            return item;
        }
    }
}
