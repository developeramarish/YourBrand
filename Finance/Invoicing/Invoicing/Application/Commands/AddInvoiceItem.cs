
using YourBrand.Invoicing.Application;
using YourBrand.Invoicing.Domain;
using YourBrand.Invoicing.Domain.Enums;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace YourBrand.Invoicing.Application.Commands;

public record AddItem(string InvoiceId, ProductType ProductType, string Description, string? ProductId, decimal UnitPrice, string Unit, decimal? Discount, double VatRate, double Quantity, bool? IsTaxDeductibleService, InvoiceItemDomesticServiceDto? DomesticService) : IRequest<InvoiceItemDto>
{
    public class Handler(IInvoicingContext context) : IRequestHandler<AddItem, InvoiceItemDto>
    {
        private readonly IInvoicingContext _context = context;

        public async Task<InvoiceItemDto> Handle(AddItem request, CancellationToken cancellationToken)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken);

            if (invoice is null)
            {
                throw new Exception("Not found");
            }

            if (invoice.Status != InvoiceStatus.Draft)
            {
                throw new Exception();
            }

            var item = invoice.AddItem(request.ProductType, request.Description, request.ProductId, request.UnitPrice, request.Unit, request.Discount, request.VatRate, request.Quantity);

            item.IsTaxDeductibleService = request.IsTaxDeductibleService.GetValueOrDefault();

            if (request.DomesticService is not null)
            {
                item.DomesticService = new Domain.Entities.InvoiceItemDomesticService(
                    request.DomesticService.Kind,
                    request.DomesticService.HomeRepairAndMaintenanceServiceType,
                    request.DomesticService.HouseholdServiceType
                );
            }

            await _context.SaveChangesAsync(cancellationToken);

            return item.ToDto();
        }
    }
}
