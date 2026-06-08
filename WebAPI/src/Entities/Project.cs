namespace WebAPI.Entities;

public sealed class Project
{

    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string Url { get; set; } = default!;
    public string TechStack { get; set; } = default!;
    public int Order { get; set; }

    public UserProfile UserProfile { get; init; } = default!;
};
