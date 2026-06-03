namespace WebAPI.Databases;

public sealed class ApplicationDbContext : DbContext
{
    public required DbSet<User> Users { get; set; }
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

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }

    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     // modelBuilder.Entity<ChatSession>()
    //     //     .Property(b => b.ChatHistory)
    //     //     .HasConversion(
    //     //     v => JsonSerializer.Serialize(v, null as JsonSerializerOptions),
    //     //     v => JsonSerializer.Deserialize<ChatHistory>(v, null as JsonSerializerOptions) ?? new ChatHistory())
    //     //     .HasColumnType("jsonb");
    //     //
    //     base.OnModelCreating(modelBuilder);
    // }
}
