namespace WebAPI.Services.Fonts;

public sealed record FontDefinition(
    string Id,
    string DisplayName,
    string Family,
    string Source,
    bool SupportsArabic,
    IReadOnlyList<string> Files
)
{
    public string CssStack =>
        $"'{Family}', 'IBM Plex Sans Arabic', 'Noto Naskh Arabic', 'DejaVu Sans', sans-serif";
}
