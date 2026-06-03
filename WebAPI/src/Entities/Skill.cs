namespace WebAPI.Entities;

public sealed record Skill(
    Guid Id,
    Guid UserProfileId,
    string Name,
    int Proficiency
    )
{
    public UserProfile UserProfile { get; init; } = default!;

};
