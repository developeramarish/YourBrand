﻿using YourBrand.Showroom.Domain.Common;

using YourBrand.Domain;

namespace YourBrand.Showroom.Domain.Entities;

public class Skill : AuditableEntity, ISoftDelete
{
    public string Id { get; set; } = null!;

    public SkillArea Area { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;

    public string? Description { get; set; }

    public DateTimeOffset? Deleted { get; set; }
    public string? DeletedById { get; set; }
    public User? DeletedBy { get; set; }

    public List<PersonProfile> PersonProfiles { get; set; } = new List<PersonProfile>();

    public List<PersonProfileSkill> PersonProfileSkills { get; set; } = new List<PersonProfileSkill>();
}