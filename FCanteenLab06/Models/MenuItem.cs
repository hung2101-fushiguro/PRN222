using System.ComponentModel.DataAnnotations;

namespace FCanteenLab06.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        [Range(1000, 1000000)] public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
        public string Stall { get; set; } = "Cơm";
    }
}
