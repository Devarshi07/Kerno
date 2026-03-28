using Microsoft.EntityFrameworkCore;
using NexusGrid.UserService.Models;

namespace NexusGrid.UserService.Data;

public sealed class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(u => u.UpdatedAt).HasDefaultValueSql("NOW()");

            entity.HasIndex(u => u.Email).IsUnique();
        });
    }
}
