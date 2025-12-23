using System.ComponentModel.DataAnnotations;

namespace FlowerDeliveryAPI.DTOs
{
    public class OrderDto
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientPhone { get; set; } = string.Empty;
        public DateOnly? DeliveryDate { get; set; }
        public TimeOnly? DeliveryTime { get; set; }
        public string? Notes { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderCreateDto
    {
        [Required]
        public int CustomerID { get; set; }

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

        [Required]
        [MinLength(1, ErrorMessage = "Заказ должен содержать хотя бы один товар")]
        public List<OrderItemCreateDto> Items { get; set; } = new List<OrderItemCreateDto>();
    }

    public class OrderUpdateDto
    {
        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(255)]
        public string? DeliveryAddress { get; set; }

        [StringLength(100)]
        public string? RecipientName { get; set; }

        [StringLength(20)]
        public string? RecipientPhone { get; set; }

        public DateOnly? DeliveryDate { get; set; }
        public TimeOnly? DeliveryTime { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}

public class OrderItemDto
{
    public int OrderItemID { get; set; }
    public int FlowerID { get; set; }
    public string FlowerName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal => Quantity * Price;
}

public class OrderItemCreateDto
{
    [Required]
    public int FlowerID { get; set; }

    [Required]
    [Range(1, 1000)]
    public int Quantity { get; set; }
}