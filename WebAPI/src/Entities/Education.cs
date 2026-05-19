

namespace WebAPI.Entities;

public class Education
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Institution { get; set; } = String.Empty;
    public string Degree { get; set; } = String.Empty;
    public int StartYear { get; set; }
    public int? EndYear { get; set; }
    public string Details { get; set; } = String.Empty;
    public int Order { get; set; }
    public UserProfile UserProfile { get; set; } = null!;
}
