namespace WebAPI.Entities;

public class Award
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Title { get; set; } = String.Empty;
    public DateTime DateAwarded { get; set; }
    public string Description { get; set; } = String.Empty;
    public int Order { get; set; }
    public UserProfile UserProfile { get; set; } = null!;
}
