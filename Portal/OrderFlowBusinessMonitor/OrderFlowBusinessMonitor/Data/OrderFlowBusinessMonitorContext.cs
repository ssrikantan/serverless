using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OrderFlowBusinessMonitor.Models
{
    public class OrderFlowBusinessMonitorContext : DbContext
    {
        public OrderFlowBusinessMonitorContext (DbContextOptions<OrderFlowBusinessMonitorContext> options)
            : base(options)
        {
        }

        public DbSet<OrderFlowBusinessMonitor.Models.Orders> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Orders>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.partnername).HasMaxLength(20);
                entity.Property(e => e.ordernumber).HasMaxLength(20);
                entity.Property(e => e.partnercode).HasMaxLength(20);
                entity.Property(e => e.item).HasMaxLength(20);
                entity.Property(e => e.qty).HasMaxLength(20);
                entity.Property(e => e.status).HasMaxLength(20);
                entity.Property(e => e.orderdate).HasMaxLength(30);
                entity.Property(e => e.deliverydate).HasMaxLength(30);
                entity.Property(e => e.amount).HasMaxLength(15);
                entity.Property(e => e.uom).HasMaxLength(20);
            });
        }
    }
}
