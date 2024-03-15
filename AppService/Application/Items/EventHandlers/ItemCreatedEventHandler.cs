﻿using System;

using YourBrand.Application.Common.Interfaces;
using YourBrand.Application.Common.Models;
using YourBrand.Domain.Events;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace YourBrand.Application.Items.EventHandlers;

public class ItemCreatedEventHandler : IDomainEventHandler<ItemCreatedEvent>
{
    private readonly IAppServiceContext _context;
    private readonly IUrlHelper _urlHelper;
    private readonly IItemsClient _itemsClient;

    public ItemCreatedEventHandler(IAppServiceContext context, IUrlHelper urlHelper, IItemsClient itemsClient)
    {
        _context = context;
        _urlHelper = urlHelper;
        _itemsClient = itemsClient;
    }

    public async Task Handle(ItemCreatedEvent notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification;

        var item = await _context.Items
            .Include(i => i.CreatedBy)
            .Include(i => i.LastModifiedBy)
            .FirstAsync(i => i.Id == domainEvent.ItemId, cancellationToken);

        var itemDto = new ItemDto(item.Id, item.Name, item.Description, _urlHelper.CreateImageUrl(item.Image), item.CommentCount, item.Created, item.CreatedBy!.ToDto()!, item.LastModified, item.LastModifiedBy?.ToDto());

        await _itemsClient.ItemAdded(itemDto);
    }
}