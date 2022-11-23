using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations.Schema;

namespace BinLite.Models
{
    [Index("DiscordId", IsUnique = true)]
    [Table("User")]
    [Keyless]
    public class User : IEntityBase
    {
        public string DiscordId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; } 
    }
}
