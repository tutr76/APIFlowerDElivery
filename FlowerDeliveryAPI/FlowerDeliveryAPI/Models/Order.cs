
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlowerDeliveryAPI.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("OrderID")]
        public int OrderID { get; set; }

        [ForeignKey("Customer")]
        [Column("CustomerID")]
        public int CustomerID { get; set; }

        public Customer? Customer { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 1000000)]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Новый";

        [Required]
        [StringLength(255)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string RecipientPhone { get; set; } = string.Empty;

        public DateOnly? DeliveryDate { get; set; }

        public TimeOnly? DeliveryTime { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}