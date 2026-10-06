namespace FCanteenLab06.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderTicketId { get; set; }
        public OrderTicket? OrderTicket { get; set; }
        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string ChangedBy { get; set; } = "Bep A";
        public DateTime ChangedAt { get; set; } = DateTime.Now;
    }
}
