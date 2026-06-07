namespace WebAPI.DTOs;

public sealed record class CertificateDto(
    Guid Id,
    string Title,
    string Issuer,
    DateTime IssueDate,
    DateTime? ExpiryDate,
    string CredentialUrl
    );
