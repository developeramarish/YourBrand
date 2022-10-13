﻿using YourBrand.Warehouse.Domain;

using MassTransit;

using MediatR;

using Microsoft.EntityFrameworkCore;
using YourBrand.Warehouse.Application.Items;
using YourBrand.Warehouse.Domain.Entities;

namespace YourBrand.Warehouse.Application.Items.Queries;

public record GetItems(int Page = 0, int PageSize = 10, string? SearchString = null, string? SortBy = null, Application.Common.Models.SortDirection? SortDirection = null) : IRequest<ItemsResult<ItemDto>>
{
    public class Handler : IRequestHandler<GetItems, ItemsResult<ItemDto>>
    {
        private readonly IWarehouseContext _context;

        public Handler(IWarehouseContext context)
        {
            _context = context;
        }

        public async Task<ItemsResult<ItemDto>> Handle(GetItems request, CancellationToken cancellationToken)
        {
            if(request.PageSize < 0) 
            {
                throw new Exception("Page Size cannot be negative.");
            }

            if(request.PageSize > 100) 
            {
                throw new Exception("Page Size must not be greater than 100.");
            }

          IQueryable<Item> result = _context
                    .Items
                    .AsNoTracking()
                    .AsQueryable();

            /*
            if (request.IndustryId is not null)
            {
                result = result.Where(p =>
                    p.Industry.Id  == request.IndustryId);
            }
            */

            if (request.SearchString is not null)
            {
                result = result.Where(p =>
                    p.Name.ToLower().Contains(request.SearchString.ToLower()));
            }

            var totalCount = await result.CountAsync(cancellationToken);

            if (request.SortBy is not null)
            {
                result = result.OrderBy(request.SortBy, request.SortDirection == Application.Common.Models.SortDirection.Desc ? Warehouse.Application.SortDirection.Descending : Warehouse.Application.SortDirection.Ascending);
            }
            else 
            {
                result = result.OrderBy(x => x.Name);
            }

            var items = await result
                .Skip(request.Page * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items2 = items.Select(cp => cp.ToDto()).ToList();

            return new ItemsResult<ItemDto>(
                items.Select(item => item.ToDto()),
                totalCount);
        }
    }
}