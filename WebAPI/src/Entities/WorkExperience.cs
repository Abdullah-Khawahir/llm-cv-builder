namespace WebAPI.Entities;

public sealed record class WorkExperience(
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
