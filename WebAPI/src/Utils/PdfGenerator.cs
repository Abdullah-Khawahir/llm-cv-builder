using System.Diagnostics;

public static class PdfGenerator
{
    public static async Task<MemoryStream> GenerateAsync(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            throw AppException.PdfGenerationFailed("Empty HTML");

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

        await using var input = process.StandardInput.BaseStream;
        await input.WriteAsync(Encoding.UTF8.GetBytes(html));
        await input.FlushAsync();
        process.StandardInput.Close();

        var output = new MemoryStream();
        var errorTask = process.StandardError.ReadToEndAsync();

        await process.StandardOutput.BaseStream.CopyToAsync(output);

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = await errorTask;
            throw AppException.PdfGenerationFailed(error);
        }

        output.Position = 0;
        return output;
    }
}
