

namespace WebAPI.DTOs;

public class LanguageDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public string Proficiency { get; set; } = String.Empty;
    public int Order { get; set; }
}
