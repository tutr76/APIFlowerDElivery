using System.ComponentModel.DataAnnotations;

namespace FlowerDeliveryAPI.DTOs
{
    public class FlowerDto
    {
        public int FlowerID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int InStock { get; set; }

        public bool IsAvailable { get; set; }
    }

    public class FlowerCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int InStock { get; set; } = 0;

        public bool IsAvailable { get; set; } = true;
    }

    public class FlowerUpdateDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [Range(0.01, 100000)]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue)]
        public int? InStock { get; set; }

        public bool? IsAvailable { get; set; }
    }
}