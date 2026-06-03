
namespace WebAPI.DTOs;

public sealed record UserProfileDto(
    Guid Id,
    string FullName,
    string Title,
    string Location,
    string PhotoUrl,
    string Email,
    string Phone,
    string LinkedInUrl,
    string GitHubUrl,
    string WebsiteUrl,
    string Summary,
    bool IsPublic,
    string Etc,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IList<WorkExperienceDto> WorkExperiences,
    IList<EducationDto> Educations,
    IList<SkillDto> Skills,
    IList<CertificateDto> Certificates,
    IList<ProjectDto> Projects,
    IList<LanguageDto> Languages,
    IList<AwardDto> Awards
    );
