﻿using YourBrand.Application.Common.Interfaces;
using YourBrand.Application.Common.Models;
using YourBrand.Domain.Events;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace YourBrand.Application.Items.EventHandlers;

public class CommentPostedEventHandler : IDomainEventHandler<CommentPostedEvent>
{
    private readonly IAppServiceContext context;

    public CommentPostedEventHandler(IAppServiceContext context)
    {
        this.context = context;
    }

    public async Task Handle(CommentPostedEvent notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification;

        var item = await context.Items
            .FirstOrDefaultAsync(i => i.Id == domainEvent.ItemId, cancellationToken);

        if (item is null) return;

        item.CommentCount = await context.Comments.CountAsync(c => c.Item.Id == domainEvent.ItemId);
    }
}