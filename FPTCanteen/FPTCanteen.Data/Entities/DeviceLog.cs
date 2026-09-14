using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTCanteen.Data.Entities
{
    public class DeviceLog
    {
        public int Id { get; set; }
        [Required, MaxLength(10)]
        public string Protocol { get; set; } = default!;//TCP, UDP, HTTP, HTTPS
        [Required, MaxLength(50)]
        public string SourceAddress { get; set; } = default!;
        public string content { get; set; } = default!;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
