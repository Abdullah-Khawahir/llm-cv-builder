namespace WebAPI.Entities;

public sealed class Language
{

    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Name { get; set; } = default!;
    public string Proficiency { get; set; } = default!;
    public int Order { get; set; }

    public UserProfile UserProfile { get; init; } = default!;
};
