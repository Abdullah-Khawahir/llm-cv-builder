
namespace WebAPI.DTOs;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = String.Empty;
    public string Title { get; set; } = String.Empty;
    public string Location { get; set; } = String.Empty;
    public string PhotoUrl { get; set; } = String.Empty;
    public string Email { get; set; } = String.Empty;
    public string Phone { get; set; } = String.Empty;
    public string LinkedInUrl { get; set; } = String.Empty;
    public string GitHubUrl { get; set; } = String.Empty;
    public string WebsiteUrl { get; set; } = String.Empty;
    public string Summary { get; set; } = String.Empty;
    public bool IsPublic { get; set; }
    public string Etc { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public IList<WorkExperienceDto> WorkExperiences { get; set; } = [];
    public IList<EducationDto> Educations { get; set; } = [];
    public IList<SkillDto> Skills { get; set; } = [];
    public IList<CertificateDto> Certificates { get; set; } = [];
    public IList<ProjectDto> Projects { get; set; } = [];
    public IList<LanguageDto> Languages { get; set; } = [];
    public IList<AwardDto> Awards { get; set; } = [];
}
