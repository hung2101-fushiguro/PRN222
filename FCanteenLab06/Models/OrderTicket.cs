using System.ComponentModel.DataAnnotations;

namespace FCanteenLab06.Models
{
    public class OrderTicket
    {
        public int Id { get; set; }
        [Required] public string TicketCode { get; set; } = "";
        public string Stall { get; set; } = "";
        public string? Note { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<TicketLine> Lines { get; set; } = new();
    }
}
