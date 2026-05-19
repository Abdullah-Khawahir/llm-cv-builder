namespace WebAPI.Entities;

public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string PhotoUrl { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string LinkedInUrl { get; set; } = null!;
    public string GitHubUrl { get; set; } = null!;
    public string WebsiteUrl { get; set; } = null!;
    public string Summary { get; set; } = null!;
    // public bool IsPublic { get; set; } = null!;
    public string Etc { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
    public ICollection<Education> Educations { get; set; } = new List<Education>();
    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Language> Languages { get; set; } = new List<Language>();
    public ICollection<Award> Awards { get; set; } = new List<Award>();
}


