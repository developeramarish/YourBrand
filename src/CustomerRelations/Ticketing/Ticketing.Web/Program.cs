using System.Diagnostics;
using System.Threading.RateLimiting;

using HealthChecks.UI.Client;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

using Serilog;

using Steeltoe.Discovery.Client;

using YourBrand;
using YourBrand.Extensions;
using YourBrand.Identity;
using YourBrand.Tenancy;
using YourBrand.Ticketing;
using YourBrand.Ticketing.Application;
using YourBrand.Ticketing.Application.Services;
using YourBrand.Ticketing.Infrastructure.Persistence;
using YourBrand.Ticketing.Web;
using YourBrand.Ticketing.Web.Middleware;

Activity.DefaultIdFormat = ActivityIdFormat.W3C;
Activity.ForceDefaultIdFormat = true;

// Define some important constants to initialize tracing with
var ServiceName = "Ticketing";
var ServiceVersion = "1.0.0";

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to container

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(builder.Configuration)
                        .Enrich.WithProperty("Application", ServiceName)
                        .Enrich.WithProperty("Environment", ctx.HostingEnvironment.EnvironmentName));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDiscoveryClient();
}

builder.Services
    .AddOpenApi(ServiceName, ApiVersions.All)
    .AddApiVersioningServices();

builder.Services.AddObservability(ServiceName, ServiceVersion, builder.Configuration);

builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:5174", "https://localhost:5001")
                          .AllowAnyHeader().AllowAnyMethod();
                      });
});

// Add services to the container.

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddUserContext()
    .AddTenantContext();

builder.Services.AddSignalR();

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

builder.Services.AddAuthorization();

builder.Services.AddAuthenticationServices(builder.Configuration);

builder.Services.AddUniverse(builder.Configuration);

builder.Services.AddTransient<ExceptionHandlingMiddleware>();

builder.Services.AddMassTransitForApp();

builder.Services.AddRateLimiter(options =>
{
    options.OnRejected = (context, cancellationToken) =>
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

            // context.Lease.GetAllMetadata().ToList()
            //    .ForEach(m => app.Logger.LogWarning($"Rate limit exceeded: {m.Key} {m.Value}"));

            return new ValueTask();
        };

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddTokenBucketLimiter("MyControllerPolicy", options =>
    {
        options.TokenLimit = 5;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

#if DEBUG
        options.QueueLimit = 10;
#else
        options.QueueLimit = 1000;
#endif

        options.ReplenishmentPeriod = TimeSpan.FromSeconds(5);
        options.TokensPerPeriod = 1;
    });
});

var app = builder.Build();

app.UseRouting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseOpenApiAndSwaggerUi();
}

app.UseCors(MyAllowSpecificOrigins);

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHubsForApp();

app.UseRateLimiter();

using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    /*
    var dbProviderName = context.Database.ProviderName;

    if (dbProviderName!.Contains("SqlServer"))
    {
        //await context.Database.EnsureDeletedAsync();
        //await context.Database.EnsureCreatedAsync();

        try
        {
            await ApplyMigrations(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred when applying migrations to the " +
                "database. Error: {Message}", ex.Message);
        }
    }
    */

    if (args.Contains("--seed"))
    {
        var tenantContext = scope.ServiceProvider.GetRequiredService<ISettableTenantContext>();
        tenantContext.SetTenantId(TenantConstants.TenantId);

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        try
        {
            await Seed.SeedData(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred seeding the " +
                "database. Error: {Message}", ex.Message);
        }

        return;
    }
}

app.Run();

static async Task ApplyMigrations(ApplicationDbContext context)
{
    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
    if (pendingMigrations.Count() > 0)
    {
        await context.Database.MigrateAsync();
    }
}

// INFO: Makes Program class visible to IntegrationTests.
public partial class Program { }