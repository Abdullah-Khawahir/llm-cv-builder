namespace WebAPI.DTOs;

public sealed record CertificateDto(
    Guid Id,
    string Title,
    string Issuer,
    DateTime IssueDate,
    DateTime? ExpiryDate,
    string CredentialUrl
    );
