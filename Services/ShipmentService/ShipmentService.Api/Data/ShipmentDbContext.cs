using Microsoft.EntityFrameworkCore;
using ShipmentService.Api.Models;

namespace ShipmentService.Api.Data;
public class ShipmentDbContext : DbContext
{
    public ShipmentDbContext(DbContextOptions<ShipmentDbContext> options) : base(options)
    {
    }

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shipment>(builder =>
        {
            builder.HasKey(s => s.OrderId);

            builder.Property(s => s.Status)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(s => s.CreatedAt)
                   .IsRequired();
        });
    }
}