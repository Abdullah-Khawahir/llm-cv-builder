namespace WebAPI.Entities;

public sealed class Award : AuditableEntity
{

    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public DateTime DateAwarded { get; set; }
    public string Description { get; set; } = default!;
    public int Order { get; set; }
    public Guid UserProfileId { get; set; }

    public UserProfile UserProfile { get; init; } = default!;
};
