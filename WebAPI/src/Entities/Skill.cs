namespace WebAPI.Entities;

public sealed class Skill : AuditableEntity
{

    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Name { get; set; } = default!;
    public int Proficiency { get; set; }

    public UserProfile UserProfile { get; init; } = default!;
};
