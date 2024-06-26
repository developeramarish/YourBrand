﻿
using YourBrand.Messenger.Domain.Common;

namespace YourBrand.Messenger.Domain.Entities;

public class MessageReceipt : AuditableEntity
{
    public string Id { get; set; } = null!;

    public Message Message { get; set; } = null!;
}