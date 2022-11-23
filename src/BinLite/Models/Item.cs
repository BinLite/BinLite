using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinLite.Models
{
    [PrimaryKey("ItemId")]
    [Table("Item")]
    public class Item : IEntityBase
    {
        public int ItemId { get; set; }
        [Required]
        public string Name { get; set; }
        public int Quantity { get; set; } = 1;
        public string Description { get; set; }
        [Required]
        public Container Container;
    }
}
