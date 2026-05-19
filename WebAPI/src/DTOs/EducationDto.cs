

namespace WebAPI.DTOs;

public class EducationDto
{
    public Guid Id { get; set; }
    public string Institution { get; set; } = String.Empty;
    public string Degree { get; set; } = String.Empty;
    public int StartYear { get; set; }
    public int? EndYear { get; set; }
    public string Details { get; } = String.Empty;  // read‑only, can be populated from entity
    public int Order { get; set; }
}
