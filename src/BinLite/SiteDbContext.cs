using BinLite.Models;

using Microsoft.EntityFrameworkCore;

namespace BinLite
{
    public class SiteDbContext : DbContext
    {
        public static string ConnectionString { get; set; }

        public DbSet<User> User { get; set; }
        public DbSet<Role> Role { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies().UseSqlServer(ConnectionString);
        }
    }
}
