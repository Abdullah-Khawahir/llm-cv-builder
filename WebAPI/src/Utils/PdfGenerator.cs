namespace WebAPI.Utils;

using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using WebAPI.Services.Fonts;

public static partial class PdfGenerator
{
    public static Task<MemoryStream> GenerateAsync(string html) =>
        GenerateAsync(html, fontCss: null);

    public static async Task<MemoryStream> GenerateAsync(string html, FontDefinition? font)
    {
        if (string.IsNullOrWhiteSpace(html))
            throw AppException.NoContentToGenerate("Empty HTML");

        return await GenerateAsync(html, font is null ? null : BuildFontCss(font)).ConfigureAwait(false);
    }

    public static async Task<MemoryStream> GenerateAsync(string html, string? fontCss)
    {
        if (string.IsNullOrWhiteSpace(html))
            throw AppException.NoContentToGenerate("Empty HTML");

        html = WithFontCss(html, fontCss);

        var startInfo = new ProcessStartInfo
        {
            FileName = "weasyprint",
            Arguments = "- -",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        process.Start();

        var input = process.StandardInput.BaseStream;
        await using (input.ConfigureAwait(false))
        {
            await input.WriteAsync(Encoding.UTF8.GetBytes(html)).ConfigureAwait(false);
            await input.FlushAsync().ConfigureAwait(false);
            process.StandardInput.Close();

            var output = new MemoryStream();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.StandardOutput.BaseStream.CopyToAsync(output).ConfigureAwait(false);

            await process.WaitForExitAsync().ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                var error = await errorTask.ConfigureAwait(false);
                throw AppException.PdfGenerationFailed(error);
            }

            output.Position = 0;
            return output;
        }
    }

    internal static string BuildFontCss(FontDefinition font)
    {
        var sb = new StringBuilder();
        foreach (var file in font.Files)
        {
            var name = Path.GetFileName(file);
            var weight =
                name.Contains("bold", StringComparison.OrdinalIgnoreCase) ? 700 :
                name.Contains("light", StringComparison.OrdinalIgnoreCase) ? 300 :
                name.Contains("medium", StringComparison.OrdinalIgnoreCase) ? 500 :
                400;

            var style = name.Contains("italic", StringComparison.OrdinalIgnoreCase) ?
                "italic" :
                "normal";

            sb.AppendLine(CultureInfo.InvariantCulture,
                    $"@font-face {{ font-family: '{font.Family}'; src: url('file://{file}'); font-weight: {weight}; font-style: {style}; }}");
        }
        sb.AppendLine($"body {{ font-family: {font.CssStack} !important; }}");
        return sb.ToString();
    }

    internal static string WithFontCss(string html, string? fontCss)
    {
        if (!html.Contains("<meta charset", StringComparison.OrdinalIgnoreCase))
        {
            html = html.Replace("<head>", "<head><meta charset=\"utf-8\">",
                StringComparison.OrdinalIgnoreCase);
        }
        if (string.IsNullOrWhiteSpace(fontCss)) return html;

        // Neutralize LLM-invented font-family declarations so the single approved
        // stack wins (ATS rule); our injected rule comes last with !important.
        html = FontFamilyRegex().Replace(html, "font-family: inherit;");
        var styleTag = $"<style data-cv-font=\"1\">{fontCss}</style>";
        if (html.Contains("</head>", StringComparison.OrdinalIgnoreCase))
            return html.Replace("</head>", styleTag + "</head>",
                StringComparison.OrdinalIgnoreCase);
        return styleTag + html;
    }

    [GeneratedRegex(@"font-family\s*:[^;}{]+;?", RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 1000)]
    private static partial Regex FontFamilyRegex();
}
