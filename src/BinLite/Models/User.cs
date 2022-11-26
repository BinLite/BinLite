using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace BinLite.Models
{
    [Index("DiscordId", IsUnique = true)]
    [Table("User")]
    public class User
    {
        public int UserId { get; set; }
        public string DiscordId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Role> UserRoles { get; set; }
    }
}
