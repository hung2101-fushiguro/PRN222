using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTCanteen.Data.Entities
{
    public class TicketLine
    {
        public int Id { get; set; }
        public int OrderTicketId { get; set; }
        public OrderTicket OrderTicket { get; set; } = default!;
        public int MenuItemId { get; set; }
        public MenuItem MenuItem { get; set; } = default!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }  // Giá tại thời điểm bán
        [MaxLength(200)]
        public string? Note { get; set; }
    }
}
