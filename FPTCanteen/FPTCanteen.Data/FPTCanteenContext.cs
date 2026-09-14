using FPTCanteen.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPTCanteen.Data
{
    public class FPTCanteenContext : DbContext
    {
        public FPTCanteenContext(DbContextOptions<FPTCanteenContext> options) : base(options) { }
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<OrderTicket> OrderTickets => Set<OrderTicket>();
        public DbSet<TicketLine> TicketLines => Set<TicketLine>();
        public DbSet<DeviceLog> DeviceLogs => Set<DeviceLog>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if(!options.IsConfigured)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
                options.UseSqlServer(config.GetConnectionString("FPTCanteenDB"));
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MenuItem>().HasData(
           new MenuItem { Id = 1, Code = "COM01", Name = "Cơm sườn nướng", Price = 25000, Unit = "Phần" },
           new MenuItem { Id = 2, Code = "COM02", Name = "Cơm gà chiên", Price = 22000, Unit = "Phần" },
           new MenuItem { Id = 3, Code = "COM03", Name = "Cơm chiên dương châu", Price = 20000, Unit = "Phần" },
           new MenuItem { Id = 4, Code = "PHO01", Name = "Phở bò", Price = 30000, Unit = "Tô" },
           new MenuItem { Id = 5, Code = "PHO02", Name = "Phở gà", Price = 28000, Unit = "Tô" },
           new MenuItem { Id = 6, Code = "MI01", Name = "Mì xào bò", Price = 22000, Unit = "Phần" },
           new MenuItem { Id = 7, Code = "MI02", Name = "Mì xào hải sản", Price = 25000, Unit = "Phần" },
           new MenuItem { Id = 8, Code = "BUN01", Name = "Bún bò Huế", Price = 30000, Unit = "Tô" },
           new MenuItem { Id = 9, Code = "BUN02", Name = "Bún chả", Price = 25000, Unit = "Phần" },
           new MenuItem { Id = 10, Code = "CAN01", Name = "Canh chua cá lóc", Price = 18000, Unit = "Phần" },
           new MenuItem { Id = 11, Code = "RAU01", Name = "Rau muống xào tỏi", Price = 12000, Unit = "Phần" },
           new MenuItem { Id = 12, Code = "TRA01", Name = "Trà đá", Price = 5000, Unit = "Ly" },
           new MenuItem { Id = 13, Code = "TRA02", Name = "Trà sen", Price = 8000, Unit = "Ly" },
           new MenuItem { Id = 14, Code = "NUOC01", Name = "Nước ngọt", Price = 10000, Unit = "Chai" },
           new MenuItem { Id = 15, Code = "SUA01", Name = "Sữa chua uống", Price = 12000, Unit = "Chai" },
           new MenuItem { Id = 16, Code = "CAFE01", Name = "Cà phê sữa", Price = 15000, Unit = "Ly" }
       );
        }
    }
}
