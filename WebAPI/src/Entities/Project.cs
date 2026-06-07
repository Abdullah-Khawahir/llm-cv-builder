namespace WebAPI.Entities;

public sealed record class Project(
    Guid Id,
    Guid UserProfileId,
    string Name,
    string Description,
    string Url,
    string TechStack,
    int Order
    )
{
    public UserProfile UserProfile { get; init; } = default!;
};
