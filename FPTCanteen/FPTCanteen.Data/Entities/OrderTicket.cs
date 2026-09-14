using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTCanteen.Data.Entities
{
    public class OrderTicket
    {
        public int Id { get; set; }
        [Required, MaxLength(20)]
        public string PosName { get; set; } = default!;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
        public ICollection<TicketLine> TicketLines { get; set; } = [];
    }
}
