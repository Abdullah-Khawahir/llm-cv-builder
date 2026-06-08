namespace WebAPI.Entities;

public sealed class Education
{

    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Institution { get; set; } = default!;
    public string Degree { get; set; } = default!;
    public int StartYear { get; set; }
    public int? EndYear { get; set; }
    public string Details { get; set; } = default!;
    public int Order { get; set; }

    public UserProfile UserProfile { get; init; } = default!;
};
