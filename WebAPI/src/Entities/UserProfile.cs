namespace WebAPI.Entities;


public sealed record UserProfile(
    Guid Id,
    Guid UserId,
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
    string Etc,
    DateTime CreatedAt,
    DateTime UpdatedAt
    )
{

    public User User { get; init; } = default!;
    public ICollection<WorkExperience> WorkExperiences { get; init; } = default!;
    public ICollection<Education> Educations { get; init; } = default!;
    public ICollection<Skill> Skills { get; init; } = default!;
    public ICollection<Certificate> Certificates { get; init; } = default!;
    public ICollection<Project> Projects { get; init; } = default!;
    public ICollection<Language> Languages { get; init; } = default!;
    public ICollection<Award> Award { get; init; } = default!;
};
