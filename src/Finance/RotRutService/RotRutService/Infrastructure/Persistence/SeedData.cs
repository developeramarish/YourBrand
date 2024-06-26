﻿namespace YourBrand.RotRutService.Infrastructure.Persistence;

public class SeedData
{
    public static async Task EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedData>>();

            var context = scope.ServiceProvider.GetRequiredService<RotRutContext>();
            await context.Database.EnsureDeletedAsync();
            //context.Database.Migrate();
            await context.Database.EnsureCreatedAsync();

            if (!context.RotRutRequests.Any())
            {
                await context.SaveChangesAsync();
            }
        }
    }
}