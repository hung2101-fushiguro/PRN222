using FCanteenLab06.Models;
using Microsoft.EntityFrameworkCore;

namespace FCanteenLab06.Data
{
    public class CanteenDbContext : DbContext
    {
        public CanteenDbContext(DbContextOptions<CanteenDbContext> o) : base(o) { }
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<OrderTicket> OrderTickets => Set<OrderTicket>();
        public DbSet<TicketLine> TicketLines => Set<TicketLine>();
        public DbSet<OrderStatusHistory> Histories => Set<OrderStatusHistory>();
    }
}
