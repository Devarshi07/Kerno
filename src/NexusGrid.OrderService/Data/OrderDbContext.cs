using Microsoft.EntityFrameworkCore;
using NexusGrid.OrderService.Models;

namespace NexusGrid.OrderService.Data;

public sealed class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(o => o.UserId).IsRequired();
            entity.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(20);
            entity.Property(o => o.TotalAmount)
                .HasPrecision(18, 2);
            entity.Property(o => o.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(o => o.UpdatedAt).HasDefaultValueSql("NOW()");

            // Store Items as JSON column
            entity.OwnsMany(o => o.Items, items =>
            {
                items.ToJson();
            });

            entity.HasIndex(o => o.UserId);
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.CreatedAt);
        });
    }
}
