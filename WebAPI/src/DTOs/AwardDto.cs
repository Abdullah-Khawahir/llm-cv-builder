

namespace WebAPI.DTOs;

public class AwardDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = String.Empty;
    public DateTime DateAwarded { get; set; }
    public string Description { get; set; } = String.Empty;
    public int Order { get; set; }
}
