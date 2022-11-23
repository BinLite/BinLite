using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinLite.Models
{
    [PrimaryKey("ContainerId")]
    [Index("Number", IsUnique = true)]
    [Table("Container")]
    public class Container : IEntityBase
    {
        public int ContainerId { get; set; }
        [Required]
        public string Number { get; set; }
        public int? ParentContainerId { get; set; }
        [Required]
        public ContainerType Type { get; set; }
        public string Description { get; set; }
    }

    public enum ContainerType
    {
        Address,
        Building,
        Zone,
        Shelf,
        Bin,
        Flag
    }
}
