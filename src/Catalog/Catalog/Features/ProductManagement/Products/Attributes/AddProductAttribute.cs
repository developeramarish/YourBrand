using MediatR;

using Microsoft.EntityFrameworkCore;

using YourBrand.Catalog.Domain.Entities;
using YourBrand.Catalog.Persistence;

namespace YourBrand.Catalog.Features.ProductManagement.Products.Attributes;

public record AddProductAttribute(long ProductId, string AttributeId, string ValueId, bool ForVariant, bool IsMainAttribute) : IRequest<ProductAttributeDto>
{
    public class Handler(CatalogContext context) : IRequestHandler<AddProductAttribute, ProductAttributeDto>
    {
        public async Task<ProductAttributeDto> Handle(AddProductAttribute request, CancellationToken cancellationToken)
        {
            var product = await context.Products
                .Include(x => x.Parent)
                .ThenInclude(x => x!.ProductAttributes)
                .ThenInclude(x => x.Attribute)
                .FirstAsync(product => product.Id == request.ProductId, cancellationToken);

            var attribute = await context.Attributes
                .Include(x => x.Values)
                .FirstOrDefaultAsync(attribute => attribute.Id == request.AttributeId, cancellationToken);

            var value = attribute!.Values
                .First();

            ProductAttribute? parentProductAttribute = null;
            if (product.Parent is not null)
            {
                parentProductAttribute = product.Parent.ProductAttributes.FirstOrDefault(x => x.AttributeId == attribute.Id);
            }

            Domain.Entities.ProductAttribute productAttribute = new()
            {
                ProductId = product.Id,
                AttributeId = attribute.Id,
                Value = value!,
                ForVariant = parentProductAttribute?.ForVariant ?? false || request.ForVariant,
                IsMainAttribute = request.IsMainAttribute
            };

            product.AddProductAttribute(productAttribute);

            await context.SaveChangesAsync(cancellationToken);

            return productAttribute.ToDto();
        }
    }
}