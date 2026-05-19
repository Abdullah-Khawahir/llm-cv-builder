

namespace WebAPI.Entities;

public class WorkExperience
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Company { get; set; } = null!;
    public string Position { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = null!;
    public int Order { get; set; }
    public UserProfile UserProfile { get; set; } = null!;
}
