﻿using YourBrand.Identity;

namespace YourBrand.Marketing.Domain.Common;

public interface ISoftDelete
{
    DateTime? Deleted { get; set; }

    UserId? DeletedById { get; set; }
}