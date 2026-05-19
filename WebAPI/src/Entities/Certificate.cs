

namespace WebAPI.Entities;

public class Certificate
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public required string Title { get; set; }
    public required string Issuer { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public required string CredentialUrl { get; set; }
    public required UserProfile UserProfile { get; set; }
}
