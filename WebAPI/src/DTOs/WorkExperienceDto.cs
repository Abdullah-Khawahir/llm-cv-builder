

namespace WebAPI.DTOs;

public class WorkExperienceDto
{
    public Guid Id { get; set; }
    public string Company { get; set; } = String.Empty;
    public string Position { get; set; } = String.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Description { get; set; } = String.Empty;
    public int Order { get; set; }
}
