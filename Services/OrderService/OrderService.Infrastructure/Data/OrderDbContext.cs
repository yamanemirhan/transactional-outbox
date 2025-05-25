using Microsoft.EntityFrameworkCore;
using OrderService.Application.Messages;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(builder =>
        {
            builder.HasKey(o => o.Id);
            builder.OwnsOne(o => o.DeliveryAddress);
            builder.Property(o => o.Status).HasConversion<string>();

            builder.HasMany<OrderItem>("_items") 
                   .WithOne(oi => oi.Order)
                   .HasForeignKey(oi => oi.OrderId)
                   .IsRequired();

            builder.Navigation("_items").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Type).IsRequired();
            builder.Property(o => o.Content).IsRequired();
            builder.Property(o => o.OccurredOn).IsRequired();
        });
    }
}
