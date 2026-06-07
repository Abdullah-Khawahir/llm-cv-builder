namespace WebAPI.Entities;

public sealed record class Education(
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
