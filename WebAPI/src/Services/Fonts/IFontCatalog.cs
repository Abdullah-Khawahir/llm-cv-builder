namespace WebAPI.Services.Fonts;

public interface IFontCatalog
{
    IReadOnlyList<FontDefinition> List();
    FontDefinition? Get(string id);
    string NormalizeId(string raw);
    string BuildFontCss(string fontId);
    Task RefreshAsync(CancellationToken ct = default);
    Task<FontDefinition> SaveUploadAsync(string fileName, Stream content, CancellationToken ct = default);
}
