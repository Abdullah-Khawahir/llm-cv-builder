namespace WebAPI.Entities;

public sealed class Certificate
{

    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Title { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string CredentialUrl { get; set; } = default!;

    public UserProfile UserProfile { get; init; } = default!;
};
