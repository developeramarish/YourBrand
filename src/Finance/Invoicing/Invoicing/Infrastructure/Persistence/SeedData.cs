﻿using YourBrand.Tenancy;

namespace YourBrand.Invoicing.Infrastructure.Persistence;

public class SeedData
{
    public static async Task EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedData>>();

            var tenantContext = scope.ServiceProvider.GetRequiredService<ISettableTenantContext>();
            tenantContext.SetTenantId(TenantConstants.TenantId);

            var context = scope.ServiceProvider.GetRequiredService<InvoicingContext>();

            await context.Database.EnsureDeletedAsync();
            //context.Database.Migrate();
            await context.Database.EnsureCreatedAsync();

            /*

            if (!context.Invoices.Any())
            {
                var invoice1 = new Invoice(DateTime.Now.Subtract(TimeSpan.FromDays(-10)), status: InvoiceStatus.Draft);
                invoice1.InvoiceNo = "1";
                invoice1.AddItem(ProductType.Good, "Item 1", 30, "pc", 0.25, 2);

                context.Invoices.Add(invoice1);

                var invoice2 = new Invoice(DateTime.Now, status: InvoiceStatus.Draft);
                invoice2.InvoiceNo = "2";
                invoice2.AddItem(ProductType.Good, "Item 1", 20, "pc", 0.25, 2);
                invoice2.AddItem(ProductType.Good, "Item 2", 100, "pc", 0.25, 3);

                context.Invoices.Add(invoice2);

                var invoice3 = new Invoice(DateTime.Now, status: InvoiceStatus.Draft);
                invoice3.InvoiceNo = "3";
                invoice3.AddItem(ProductType.Service, "Konsultarbete", 890, "pc", 0.25, 40);

                context.Invoices.Add(invoice3);

                await context.SaveChangesAsync();
            }

            */
        }
    }
}