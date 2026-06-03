namespace WebAPI.Entities;

public sealed record Language(
    Guid Id,
    Guid UserProfileId,
    string Name,
    string Proficiency,
    int Order
    )
{
    public UserProfile UserProfile { get; init; } = default!;
};
