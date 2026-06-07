namespace WebAPI.Entities;

public sealed record class Certificate(
    Guid Id,
    Guid UserProfileId,
    string Title,
    string Issuer,
    DateTime IssueDate,
    DateTime? ExpiryDate,
    string CredentialUrl
    )
{
    public UserProfile UserProfile { get; init; } = default!;
};
