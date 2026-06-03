namespace WebAPI.Entities;

public sealed record Award(
    Guid Id,
    string Title,
    DateTime DateAwarded,
    string Description,
    int Order,
    Guid UserProfileId
    )
{
    public UserProfile UserProfile { get; init; } = default!;

};
