using System.ComponentModel.DataAnnotations;

namespace FCanteenLab06.Models
{
    public class TicketLine
    {
        public int Id { get; set; }
        public int OrderTicketId { get; set; }
        public OrderTicket? OrderTicket { get; set; }
        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }
        [Range(1, 99)] public int Quantity { get; set; }
    }
}
