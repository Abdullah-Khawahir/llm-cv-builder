namespace WebAPI.Utils;

using System.Diagnostics;

public static class PdfGenerator
{
    public static async Task<MemoryStream> GenerateAsync(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            throw AppException.NoContentToGenerate("Empty HTML");

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
}
