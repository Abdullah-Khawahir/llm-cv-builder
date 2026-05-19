namespace WebAPI.Entities;

public class Skill
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Name { get; set; } = String.Empty;
    public int Proficiency { get; set; }
    public UserProfile UserProfile { get; set; } = null!;
}
