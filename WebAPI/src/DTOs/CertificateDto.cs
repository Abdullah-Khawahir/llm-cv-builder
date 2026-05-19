

namespace WebAPI.DTOs;

public class CertificateDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = String.Empty;
    public string Issuer { get; set; } = String.Empty;
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string CredentialUrl { get; set; } = String.Empty;
}
