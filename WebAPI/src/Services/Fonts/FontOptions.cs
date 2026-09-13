namespace WebAPI.Services.Fonts;

public sealed class FontOptions
{
    public string Directory { get; set; } = "fonts";
    public bool DownloadGoogleFonts { get; set; } = true;
    public int MaxUploadMb { get; set; } = 8;
    public string DefaultFontId { get; set; } = "dejavu";
    public string GoogleFontsApiKey { get; set; } = string.Empty;
    public string[] GoogleFontFamilies { get; set; } = [];
}
