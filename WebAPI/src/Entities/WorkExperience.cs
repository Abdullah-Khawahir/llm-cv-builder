namespace WebAPI.Entities;

public sealed class WorkExperience
{

    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Company { get; set; } = default!;
    public string Position { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = default!;
    public int Order { get; set; }

    public UserProfile UserProfile { get; init; } = default!;
};
