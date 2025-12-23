using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerDeliveryAPI.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        [Column("OrderItemID")]
        public int OrderItemID { get; set; }

        [ForeignKey("Order")]
        [Column("OrderID")]
        public int OrderID { get; set; }

        public Order? Order { get; set; }

        [ForeignKey("Flower")]
        [Column("FlowerID")]
        public int FlowerID { get; set; }

        public Flower? Flower { get; set; }

        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 100000)]
        public decimal Price { get; set; }
    }
}