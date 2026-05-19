namespace WebAPI.Entities;

public class Project
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public string Url { get; set; } = String.Empty;
    public string TechStack { get; set; } = String.Empty;
    public int Order { get; set; }
    public UserProfile UserProfile { get; set; } = null!;
}
