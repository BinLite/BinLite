using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinLite.Models
{
    [Table("Role")]
    [Index("Name", IsUnique = true)]
    public class Role
    {
        public int RoleId { get; set; }
        [Required]
        public string Name { get; set; }

        public virtual Role ParentRole { get; set; }

        public virtual ICollection<User> RoleUsers { get; set; }
    }
}
