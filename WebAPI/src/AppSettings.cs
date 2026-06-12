namespace WebAPI;

public sealed class AppSettings
{
    [Required]
    public ConnectionStrings ConnectionStrings { get; set; } = default!;

    [Required]
    public Jwt Jwt { get; set; } = default!;

    [Required]
    public Google Google { get; set; } = default!;


    [Required]
    public Minio Minio { get; set; } = default!;
}

public sealed record class ConnectionStrings(string DefaultConnection);

public sealed record class Google(
        string ClientId,
        string ClientSecret
        );
public sealed record class Jwt(
    [property: MinLength(32)] string Key,
    string Audience,
    string Issuer
    );

public sealed record class Minio(
    string Endpoint,
    string AccessKey,
    string SecretKey
    );

