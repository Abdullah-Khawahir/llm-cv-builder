namespace WebAPI.Entities;

public sealed record Education(
    Guid Id,
    Guid UserProfileId,
    string Institution,
    string Degree,
    int StartYear,
    int? EndYear,
    string Details,
    int Order
    )
{
    public UserProfile UserProfile { get; init; } = default!;
};
