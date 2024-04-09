﻿using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using YourBrand.Domain;
using YourBrand.Identity;
using YourBrand.RotRutService.Domain;
using YourBrand.RotRutService.Domain.Common;
using YourBrand.RotRutService.Domain.Entities;
using YourBrand.RotRutService.Infrastructure.Persistence.Interceptors;
using YourBrand.RotRutService.Infrastructure.Persistence.Outbox;
using YourBrand.Tenancy;

namespace YourBrand.RotRutService.Infrastructure.Persistence;

public class RotRutContext(
    DbContextOptions<RotRutContext> options,
    AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor) : DbContext(options), IRotRutContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.AddInterceptors(auditableEntitySaveChangesInterceptor);

#if DEBUG
        optionsBuilder.EnableSensitiveDataLogging();
#endif
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RotRutContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddTenantIdConverter();
        configurationBuilder.AddOrganizationIdConverter();
        configurationBuilder.AddUserIdConverter();
    }

    public DbSet<RotRutRequest> RotRutRequests { get; set; } = null!;

    public DbSet<RotRutCase> RotRutCases { get; set; } = null!;

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entities = ChangeTracker
                        .Entries<Entity>()
                        .Where(e => e.Entity.DomainEvents.Any())
                        .Select(e => e.Entity);

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .OrderBy(e => e.Timestamp)
            .ToList();

        var outboxMessages = domainEvents.Select(domainEvent =>
        {
            return new OutboxMessage()
            {
                Id = domainEvent.Id,
                OccurredOnUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Content = JsonConvert.SerializeObject(
                    domainEvent,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    })
            };
        }).ToList();

        this.Set<OutboxMessage>().AddRange(outboxMessages);

        return await base.SaveChangesAsync(cancellationToken);
    }
}