﻿using System.Linq.Expressions;

using LinqKit;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;

using YourBrand.Customers.Domain;
using YourBrand.Customers.Domain.Common;
using YourBrand.Customers.Domain.Entities;
using YourBrand.Customers.Infrastructure.Persistence.Interceptors;
using YourBrand.Customers.Infrastructure.Persistence.Outbox;
using YourBrand.Domain;
using YourBrand.Identity;
using YourBrand.Tenancy;

namespace YourBrand.Customers.Infrastructure.Persistence;

public class CustomersContext(
    DbContextOptions<CustomersContext> options,
    ITenantContext tenantContext,
    AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor) : DbContext(options), ICustomersContext
{
    private readonly TenantId _tenantId = tenantContext.TenantId.GetValueOrDefault();

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

        modelBuilder.HasSequence<int>("CustomerIds");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomersContext).Assembly);

        ConfigQueryFilterForEntity(modelBuilder);
    }

    private void ConfigQueryFilterForEntity(ModelBuilder modelBuilder)
    {
        foreach (var clrType in modelBuilder.Model
            .GetEntityTypes()
            .Select(entityType => entityType.ClrType))
        {
            if (clrType.BaseType != typeof(object))
            {
                continue;
            }

            try
            {
                var entityTypeBuilder = modelBuilder.Entity(clrType);

                var parameter = Expression.Parameter(clrType, "entity");

                List<Expression> queryFilters = new();

                if (TenancyQueryFilter.CanApplyTo(clrType))
                {
                    var tenantFilter = TenancyQueryFilter.GetFilter(() => _tenantId!);

                    queryFilters.Add(
                        Expression.Invoke(tenantFilter, Expression.Convert(parameter, typeof(IHasTenant))));
                }

                if (SoftDeleteQueryFilter.CanApplyTo(clrType))
                {
                    var softDeleteFilter = SoftDeleteQueryFilter.GetFilter();

                    queryFilters.Add(
                        Expression.Invoke(softDeleteFilter, Expression.Convert(parameter, typeof(ISoftDelete))));
                }

                Expression? queryFilter = null;

                foreach (var qf in queryFilters)
                {
                    if (queryFilter is null)
                    {
                        queryFilter = qf;
                    }
                    else
                    {
                        queryFilter = Expression.AndAlso(
                            queryFilter,
                            qf)
                            .Expand();
                    }
                }

                if (queryFilters.Count == 0)
                {
                    continue;
                }

                var queryFilterLambda = Expression.Lambda(queryFilter.Expand(), parameter);

                entityTypeBuilder.HasQueryFilter(queryFilterLambda);
            }
            catch (InvalidOperationException exc)
                when (exc.Message.Contains("cannot be configured as non-owned because it has already been configured as a owned"))
            {
                Console.WriteLine("Skipping previously configured owned type");
            }
            catch (InvalidOperationException exc)
                when (exc.Message.Contains("cannot be added to the model because its CLR type has been configured as a shared type"))
            {
                Console.WriteLine("Skipping previously configured shared type");
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddTenantIdConverter();
        configurationBuilder.AddOrganizationIdConverter();
        configurationBuilder.AddUserIdConverter();
    }

    public DbSet<Customer> Customers { get; set; } = null!;

    public DbSet<Person> Persons { get; set; } = null!;

    public DbSet<Organization> Organizations { get; set; } = null!;

    public DbSet<Address> Addresses { get; set; } = null!;

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
                Id = Guid.NewGuid(),
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