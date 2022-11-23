using BinLite.Models;

using Microsoft.EntityFrameworkCore;

namespace BinLite
{
    public class SiteDbContext : DbContext
    {
        public static string ConnectionString { get; set; }

        public DbSet<Item> Item { get; set; }
        public DbSet<Container> Container { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<ActivityLog> ActivityLog { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }
}
