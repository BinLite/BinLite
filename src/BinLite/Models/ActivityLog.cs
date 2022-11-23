using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinLite.Models
{
    [PrimaryKey("ActivityLogId")]
    [Table("ActivityLog")]
    public class ActivityLog : IEntityBase
    {
        public int ActivityLogId { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
    }
}
