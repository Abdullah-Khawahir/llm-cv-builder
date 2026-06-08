namespace WebAPI.Entities;


public sealed class UserProfile
{

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? FullName { get; set; } = default!;
    public string? Title { get; set; } = default!;
    public string? Location { get; set; } = default!;
    public string? PhotoUrl { get; set; } = default!;
    public string? Email { get; set; } = default!;
    public string? Phone { get; set; } = default!;
    public string? LinkedInUrl { get; set; } = default!;
    public string? GitHubUrl { get; set; } = default!;
    public string? WebsiteUrl { get; set; } = default!;
    public string? Summary { get; set; } = default!;
    public string? Etc { get; set; } = default!;



    public User User { get; init; } = default!;
    public ICollection<WorkExperience> WorkExperiences { get; init; } = default!;
    public ICollection<Education> Educations { get; init; } = default!;
    public ICollection<Skill> Skills { get; init; } = default!;
    public ICollection<Certificate> Certificates { get; init; } = default!;
    public ICollection<Project> Projects { get; init; } = default!;
    public ICollection<Language> Languages { get; init; } = default!;
    public ICollection<Award> Award { get; init; } = default!;
};
