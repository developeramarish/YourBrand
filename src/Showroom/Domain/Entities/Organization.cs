using YourBrand.Domain;
using YourBrand.Identity;
using YourBrand.Showroom.Domain.Common;
using YourBrand.Showroom.Domain.ValueObjects;
using YourBrand.Tenancy;

namespace YourBrand.Showroom.Domain.Entities;

public class Organization : AuditableEntity, IHasTenant, ISoftDelete
{
    public OrganizationId Id { get; set; }

    public TenantId TenantId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public Address Address { get; set; } = null!;

    public ICollection<PersonProfile> PersonProfiles { get; set; } = null!;

    public DateTimeOffset? Deleted { get; set; }
    public UserId? DeletedById { get; set; }
    public User? DeletedBy { get; set; }
}