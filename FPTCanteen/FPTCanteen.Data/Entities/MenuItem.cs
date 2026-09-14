using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTCanteen.Data.Entities
{
    public class MenuItem
    {
        public int Id { get; set; }
        [Required, MaxLength(10)]
        public string Code { get; set; } = default!;
        [Required, MaxLength(100)]
        public string Name { get; set; } = default!;
        public decimal Price { get; set; }
        [Required, MaxLength(20)]
        public string Unit { get; set; } = default!;
        public bool IsAvailable { get; set; } = true;
    }
}
