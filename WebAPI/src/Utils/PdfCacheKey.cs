namespace WebAPI.Utils;

public static class PdfCacheKey
{
    public static string FromHtml(string html)
    {
        var normalized = html.Trim();

        var hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(normalized));

        return Convert.ToHexString(hash)[..16];
    }
}
