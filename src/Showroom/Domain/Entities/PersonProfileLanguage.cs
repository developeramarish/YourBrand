using YourBrand.Showroom.Domain.Common;
using YourBrand.Showroom.Domain.Enums;
using YourBrand.Tenancy;

using YourBrand.Domain;

namespace YourBrand.Showroom.Domain.Entities;

public class PersonProfileLanguage : AuditableEntity, IHasTenant, ISoftDelete
{
    public string Id { get; set; } = null!;

    public TenantId TenantId { get; set; } = null!;

    public PersonProfile PersonProfile { get; set; } = null!;

    public string LanguageId { get; set; } = null!;

    public Language Language { get; set; } = null!;

    public LanguageProficiency LanguageProficiency { get; set; }

    public DateTimeOffset? Deleted { get; set; }
    public string? DeletedById { get; set; }
    public User? DeletedBy { get; set; }
}