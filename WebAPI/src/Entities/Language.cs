namespace WebAPI.Entities;

public sealed record class Language(
    Guid Id,
    Guid UserProfileId,
    string Name,
    string Proficiency,
    int Order
    )
{
    public UserProfile UserProfile { get; init; } = default!;
};
