namespace WebAPI;

public sealed class AppSettings
{
    [Required]
    public ConnectionStrings ConnectionStrings { get; set; } = default!;

    [Required]
    public Jwt Jwt { get; set; } = default!;

    [Required]
    public Google Google { get; set; } = default!;
}

public sealed record ConnectionStrings(string DefaultConnection);

public sealed record Google(
        string ClientId,
        string ClientSecret
        );
public sealed record Jwt(
    [property: MinLength(32)] string Key,
    string Audience,
    string Issuer
    );

