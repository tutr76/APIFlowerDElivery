using Microsoft.EntityFrameworkCore;
using FlowerDeliveryAPI.Models;

namespace FlowerDeliveryAPI.Data
{
    public class FlowerDeliveryContext : DbContext
    {
        public FlowerDeliveryContext(DbContextOptions<FlowerDeliveryContext> options)
            : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Flower> Flowers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Flower)
                .WithMany()
                .HasForeignKey(oi => oi.FlowerID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}