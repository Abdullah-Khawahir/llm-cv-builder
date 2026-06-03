namespace WebAPI.Entities;

public sealed record WorkExperience(
    Guid Id,
    Guid UserProfileId,
    string Company,
    string Position,
    DateTime StartDate,
    DateTime? EndDate,
    string Description,
    int Order
    )
{
    public UserProfile UserProfile { get; init; } = default!;
};
