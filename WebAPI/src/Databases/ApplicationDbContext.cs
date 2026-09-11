using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace WebAPI.Databases;

public sealed class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    // public required DbSet<User> Users { get; set; }
    public required DbSet<Award> Awards { get; set; }
    public required DbSet<Certificate> Certificates { get; set; }
    public required DbSet<Education> Educations { get; set; }
    public required DbSet<Language> Languages { get; set; }
    public required DbSet<Profile> Profiles { get; set; }
    public required DbSet<Project> Projects { get; set; }
    public required DbSet<Skill> Skills { get; set; }
    public required DbSet<UserProfile> UserProfiles { get; set; }
    public required DbSet<WorkExperience> WorkExperiences { get; set; }
    public required DbSet<ChatSession> ChatSessions { get; set; }

    public required DbSet<Payment> Payments { get; set; }
    public required DbSet<PaymentEvent> PaymentEvents { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PaymentEvent>()
                .Property(p => p.Type)
                .HasConversion<string>();
    }
}
