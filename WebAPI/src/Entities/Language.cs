

namespace WebAPI.Entities;

public class Language
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Proficiency { get; set; } = String.Empty;
    public int Order { get; set; }
    public UserProfile UserProfile { get; set; } = null!;
}
