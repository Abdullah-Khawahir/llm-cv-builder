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



    public User User { get; set; } = default!;
    public ICollection<WorkExperience> WorkExperiences { get; set; } = default!;
    public ICollection<Education> Educations { get; set; } = default!;
    public ICollection<Skill> Skills { get; set; } = default!;
    public ICollection<Certificate> Certificates { get; set; } = default!;
    public ICollection<Project> Projects { get; set; } = default!;
    public ICollection<Language> Languages { get; set; } = default!;
    public ICollection<Award> Award { get; set; } = default!;
};
