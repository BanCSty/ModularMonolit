using Microsoft.EntityFrameworkCore;
using Shared.Outbox;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence;

public class UserDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<UserOrderSummary> UserOrderSummary { get; set; }

    public UserDbContext(DbContextOptions<UserDbContext> options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Payload).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.PublishedAt);
            entity.Property(e => e.RetryCount).IsRequired();
            entity.Property(e => e.Error).HasMaxLength(1000);

            entity.HasIndex(e => e.PublishedAt);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<UserOrderSummary>(entity =>
        {
            entity.ToTable("UserOrderSummaries");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.TotalSpent)
                  .HasConversion<decimal>();
        });

        base.OnModelCreating(modelBuilder);
    }
}