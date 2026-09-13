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

    public FontSettings Fonts { get; set; } = new();
}

public sealed class FontSettings
{
    public string Directory { get; set; } = "fonts";
    public bool DownloadGoogleFonts { get; set; } = true;
    public int MaxUploadMb { get; set; } = 8;
    public string DefaultFontId { get; set; } = "dejavu";

    /// <summary>
    /// Google Fonts Developer API key (https://developers.google.com/fonts/docs/developer_api).
    /// When set, the families in <see cref="GoogleFontFamilies"/> are downloaded via the
    /// official API (with correct Arabic-subset detection). Without a key, a small set of
    /// known-good GitHub mirrors is used instead and remaining families resolve via
    /// system fonts (Liberation / Noto apt packages).
    /// Env override: Fonts__GoogleFontsApiKey
    /// </summary>
    public string GoogleFontsApiKey { get; set; } = string.Empty;

    public string[] GoogleFontFamilies { get; set; } =
    [
        "Inter", "Cairo", "IBM Plex Sans Arabic",
        "Noto Sans", "Noto Serif", "Arimo", "Tinos", "Roboto",
    ];
}

public sealed record ConnectionStrings(string DefaultConnection);

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

