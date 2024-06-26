using Microsoft.EntityFrameworkCore;

using YourBrand.Domain.Outbox;

namespace YourBrand.Domain.Persistence;

public abstract class DomainDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DomainDbContext).Assembly);
    }

#nullable disable

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public DbSet<OutboxMessageConsumer> OutboxMessageConsumers { get; set; }

#nullable restore
}