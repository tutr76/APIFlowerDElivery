using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerDeliveryAPI.Models
{
    [Table("Flowers")]
    public class Flower
    {
        [Key]
        [Column("FlowerID")]
        public int FlowerID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        [Column("InStock")]
        [Range(0, int.MaxValue)]
        public int InStock { get; set; } = 0;

        public bool IsAvailable { get; set; } = true;
    }
}