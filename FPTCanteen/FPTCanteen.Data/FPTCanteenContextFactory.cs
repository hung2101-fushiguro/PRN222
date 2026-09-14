using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTCanteen.Data
{
    public class FPTCanteenContextFactory : IDesignTimeDbContextFactory<FPTCanteenContext>
    {
        public FPTCanteenContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FPTCanteenContext>();
            optionsBuilder.UseSqlServer(
                "Server=SATORU\\SQLEXPRESS;Database=PRN222_Lab_FPTCanteen;User Id=sa;Password=hungsatoru;TrustServerCertificate=True;MultipleActiveResultSets=True");
            return new FPTCanteenContext(optionsBuilder.Options);
        }
    }
}
