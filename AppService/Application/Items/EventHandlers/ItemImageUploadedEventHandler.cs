﻿
using YourBrand.Application.Common.Interfaces;
using YourBrand.Application.Common.Models;
using YourBrand.Domain.Events;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace YourBrand.Application.Items.EventHandlers;

public class ItemImageUploadedEventHandler : IDomainEventHandler<ItemImageUploadedEvent>
{
    private readonly IAppServiceContext _context;
    private readonly IUrlHelper _urlHelper;
    private readonly IItemsClient _itemsClient;

    public ItemImageUploadedEventHandler(IAppServiceContext context, IUrlHelper urlHelper, IItemsClient itemsClient)
    {
        _context = context;
        _urlHelper = urlHelper;
        _itemsClient = itemsClient;
    }

    public async Task Handle(ItemImageUploadedEvent notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification;

        var item = await _context.Items.FirstAsync(i => i.Id == domainEvent.Id, cancellationToken);

        await _itemsClient.ImageUploaded(domainEvent.Id, _urlHelper.CreateImageUrl(item.Image)!);
    }
}