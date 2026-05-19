namespace WebAPI.DTOs;

public class SkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public int Proficiency { get; set; }
}
