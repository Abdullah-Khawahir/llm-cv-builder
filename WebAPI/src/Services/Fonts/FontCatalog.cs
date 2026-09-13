using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace WebAPI.Services.Fonts;

public sealed class FontCatalog : IFontCatalog
{
    private readonly ILogger<FontCatalog> _log;
    private readonly FontOptions _options;
    private readonly string _dir;
    private List<FontDefinition> _fonts = new() { DefaultDejaVu() };
    private readonly object _lock = new();

    /// <summary>
    /// No-key fallback: known-good raw files from github.com/google/fonts.
    /// Used only when <see cref="FontOptions.GoogleFontsApiKey"/> is empty.
    /// </summary>
    private static readonly Dictionary<string, string[]> GithubMirrorFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cairo"] = new[]
        {
            "ofl/cairo/Cairo[slnt,wght].ttf",
        },
        ["ibm-plex-sans-arabic"] = new[]
        {
            "ofl/ibmplexsansarabic/IBMPlexSansArabic-Regular.ttf",
            "ofl/ibmplexsansarabic/IBMPlexSansArabic-Bold.ttf",
        },
        ["inter"] = new[]
        {
            "ofl/inter/Inter[opsz,wght].ttf",
        },
    };

    public FontCatalog(ILogger<FontCatalog> log, IOptions<FontOptions> options, IWebHostEnvironment env)
    {
        _log = log;
        _options = options.Value;
        _dir = Path.IsPathRooted(_options.Directory)
            ? _options.Directory
            : Path.Combine(env.ContentRootPath, _options.Directory);
        Directory.CreateDirectory(_dir);
        Scan();
        // Best-effort Google pre-download, never fatal.
        if (_options.DownloadGoogleFonts)
            _ = Task.Run(() => TryDownloadGoogleFontsAsync(CancellationToken.None));
    }

    public IReadOnlyList<FontDefinition> List()
    {
        lock (_lock) return _fonts.ToList();
    }

    public FontDefinition? Get(string id)
    {
        var norm = NormalizeId(id);
        lock (_lock) return _fonts.FirstOrDefault(f => string.Equals(f.Id, norm, StringComparison.Ordinal))
            ?? _fonts.FirstOrDefault(f => string.Equals(f.Id, _options.DefaultFontId, StringComparison.Ordinal))
            ?? _fonts.FirstOrDefault();
    }

    public string NormalizeId(string raw) =>
        (raw ?? string.Empty).Trim().ToLowerInvariant()
            .Replace(' ', '-').Replace('_', '-');

    public string BuildFontCss(string fontId)
    {
        var font = Get(fontId) ?? DefaultDejaVu();
        var sb = new StringBuilder();
        foreach (var file in font.Files)
        {
            var name = Path.GetFileName(file);
            var weight = name.Contains("bold", StringComparison.OrdinalIgnoreCase) ? 700
                : name.Contains("light", StringComparison.OrdinalIgnoreCase) ? 300
                : name.Contains("medium", StringComparison.OrdinalIgnoreCase) ? 500 : 400;
            var style = name.Contains("italic", StringComparison.OrdinalIgnoreCase) ? "italic" : "normal";
            sb.AppendLine(CultureInfo.InvariantCulture,
                    $"@font-face {{ font-family: '{font.Family}'; src: url('file://{file}'); font-weight: {weight}; font-style: {style}; }}");
        }
        sb.AppendLine($"body {{ font-family: {font.CssStack} !important; }}");
        return sb.ToString();
    }

    public async Task RefreshAsync(CancellationToken ct = default)
    {
        Scan();
        if (_options.DownloadGoogleFonts)
            await TryDownloadGoogleFontsAsync(ct).ConfigureAwait(false);
        RefreshFontconfig();
    }

    public async Task<FontDefinition> SaveUploadAsync(string fileName, Stream content, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext is not (".ttf" or ".otf" or ".woff" or ".woff2"))
            throw AppException.BadRequest($"Unsupported font extension '{ext}'. Use .ttf/.otf/.woff/.woff2.");

        var maxBytes = (long)_options.MaxUploadMb * 1024 * 1024;
        var id = NormalizeId(Path.GetFileNameWithoutExtension(fileName));
        if (string.IsNullOrWhiteSpace(id)) id = $"upload-{Guid.NewGuid():N}"[..12];

        // Validate magic bytes BEFORE touching disk so rejections leave no junk.
        var header = new byte[4];
        var headerRead = await content.ReadAsync(header.AsMemory(0, 4), ct).ConfigureAwait(false);
        if (headerRead == 4 && !LooksLikeFont(header))
            throw AppException.BadRequest("File does not look like a font (bad magic bytes).");

        var targetDir = Path.Combine(_dir, "uploaded", id);
        Directory.CreateDirectory(targetDir);
        var target = Path.Combine(targetDir, Path.GetFileName(fileName));

        try
        {
            var fs = File.Create(target);
            await using (fs.ConfigureAwait(false))
            {
                var total = 0L;
            if (headerRead > 0)
            {
                await fs.WriteAsync(header.AsMemory(0, headerRead), ct).ConfigureAwait(false);
                total += headerRead;
            }
            var buffer = new byte[81920];
            int n;
            while ((n = await content.ReadAsync(buffer, ct).ConfigureAwait(false)) > 0)
            {
                total += n;
                if (total > maxBytes) throw AppException.BadRequest($"Font exceeds {_options.MaxUploadMb}MB.");
                await fs.WriteAsync(buffer.AsMemory(0, n), ct).ConfigureAwait(false);
            }
            }
        }
        catch
        {
            try { File.Delete(target); } catch { }
            throw;
        }

        Scan();
        RefreshFontconfig();
        return Get($"uploaded-{id}") ?? Get(id) ?? List().Last();
    }

    private static bool LooksLikeFont(byte[] h) =>
        (h[0] == 0x00 && h[1] == 0x01 && h[2] == 0x00 && h[3] == 0x00) // ttf
        || (h[0] == (byte)'O' && h[1] == (byte)'T' && h[2] == (byte)'T' && h[3] == (byte)'O') // otf
        || (h[0] == (byte)'w' && h[1] == (byte)'O') // wOFF / wOF2
        || (h[0] == (byte)'t' && h[1] == (byte)'r' && h[2] == (byte)'u' && h[3] == (byte)'e'); // mac ttf

    private void Scan()
    {
        var found = new List<FontDefinition> { DefaultDejaVu() };
        try
        {
            // 1. Manifest entries (id -> display metadata, files resolved from disk).
            foreach (var m in ReadManifest())
            {
                if (found.Any(f => string.Equals(f.Id, m.Id, StringComparison.OrdinalIgnoreCase))) continue;
                var files = CollectFiles(Path.Combine(_dir, m.Id));
                if (m.Id.StartsWith("uploaded-", StringComparison.OrdinalIgnoreCase))
                    files = CollectFiles(Path.Combine(_dir, m.Id["uploaded-".Length..]));
                found.Add(new FontDefinition(m.Id, m.DisplayName, m.Family, "local", m.SupportsArabic, files));
            }

            // 2. Any other subdir with font files = plug-and-play, no manifest needed.
            foreach (var sub in Directory.GetDirectories(_dir))
            {
                var id = NormalizeId(Path.GetFileName(sub));
                if (found.Any(f => string.Equals(f.Id, id, StringComparison.Ordinal))) continue;
                var files = CollectFiles(sub);
                if (files.Count == 0) continue;
                var family = ToDisplay(id);
                found.Add(new FontDefinition(id, family, family, "local",
                    GuessArabic(id + family), files));
            }

            // 3. Uploaded subfolder flattening: uploaded/<id>/* -> uploaded-<id>.
            var upRoot = Path.Combine(_dir, "uploaded");
            if (Directory.Exists(upRoot))
                foreach (var sub in Directory.GetDirectories(upRoot))
                {
                    var id = "uploaded-" + NormalizeId(Path.GetFileName(sub));
                    if (found.Any(f => string.Equals(f.Id, id, StringComparison.Ordinal))) continue;
                    var files = CollectFiles(sub);
                    if (files.Count == 0) continue;
                    var family = ToDisplay(Path.GetFileName(sub));
                    found.Add(new FontDefinition(id, family, family, "uploaded",
                        GuessArabic(id + family), files));
                }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Font scan failed, keeping previous list");
            return;
        }
        lock (_lock) _fonts = found;
        _log.LogInformation("Font catalog: {Ids}", string.Join(",", found.Select(f => f.Id)));
    }

    private IReadOnlyList<string> CollectFiles(string folder)
    {
        if (!Directory.Exists(folder)) return Array.Empty<string>();
        return Directory.GetFiles(folder)
            .Where(f => f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)
                || f.EndsWith(".otf", StringComparison.OrdinalIgnoreCase)
                || f.EndsWith(".woff", StringComparison.OrdinalIgnoreCase)
                || f.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private async Task TryDownloadGoogleFontsAsync(CancellationToken ct)
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        if (!string.IsNullOrWhiteSpace(_options.GoogleFontsApiKey))
            await DownloadViaGoogleFontsApiAsync(http, ct).ConfigureAwait(false);
        else
            await DownloadViaGithubMirrorAsync(http, ct).ConfigureAwait(false);
        Scan();
        RefreshFontconfig();
    }

    /// <summary>
    /// Official Google Fonts Developer API path. Downloads regular/bold
    /// (+italic variants when present) for every family in
    /// <see cref="FontOptions.GoogleFontFamilies"/> and detects Arabic
    /// support from the family's subsets list.
    /// Get a key: https://developers.google.com/fonts/docs/developer_api
    /// </summary>
    private async Task DownloadViaGoogleFontsApiAsync(HttpClient http, CancellationToken ct)
    {
        JsonDocument api;
        try
        {
            var url = "https://www.googleapis.com/webfonts/v1/webfonts?key="
                + Uri.EscapeDataString(_options.GoogleFontsApiKey) + "&sort=popularity";
            using var res = await http.GetAsync(url, ct).ConfigureAwait(false);
            if (!res.IsSuccessStatusCode)
            {
                _log.LogWarning("Google Fonts API failed: {Status}. Falling back to GitHub mirror.", res.StatusCode);
                await DownloadViaGithubMirrorAsync(http, ct).ConfigureAwait(false);
                return;
            }
            var stream = await res.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            await using (stream.ConfigureAwait(false))
            {
                api = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Google Fonts API unreachable (offline ok). Falling back to GitHub mirror.");
            await DownloadViaGithubMirrorAsync(http, ct).ConfigureAwait(false);
            return;
        }

        using (api)
        {
            var wanted = _options.GoogleFontFamilies
                .Select(f => (Id: NormalizeId(f), Family: f.Trim()))
                .Where(f => !string.IsNullOrEmpty(f.Family))
                .ToDictionary(f => f.Family, f => f.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var item in api.RootElement.GetProperty("items").EnumerateArray())
            {
                if (!item.TryGetProperty("family", out var familyEl)) continue;
                var family = familyEl.GetString() ?? string.Empty;
                if (!wanted.TryGetValue(family, out var id)) continue;
                if (!item.TryGetProperty("files", out var filesEl)) continue;

                var arabic = item.TryGetProperty("subsets", out var subsetsEl)
                    && subsetsEl.EnumerateArray()
                        .Any(s => string.Equals(s.GetString(), "arabic", StringComparison.OrdinalIgnoreCase));

                var variants = new[] { "regular", "bold", "italic", "bolditalic", "medium", "semibold" };
                var targetDir = Path.Combine(_dir, id);
                foreach (var variant in variants)
                {
                    if (!filesEl.TryGetProperty(variant, out var urlEl)) continue;
                    var fileUrl = urlEl.GetString();
                    if (string.IsNullOrWhiteSpace(fileUrl)) continue;
                    try
                    {
                        Directory.CreateDirectory(targetDir);
                        var dest = Path.Combine(targetDir, $"{family}-{variant}.ttf");
                        if (new FileInfo(dest).Length > 0) continue;
                        using var dl = await http.GetAsync(fileUrl, ct).ConfigureAwait(false);
                        if (!dl.IsSuccessStatusCode) continue;
                        var fs = File.Create(dest);
                        await using (fs.ConfigureAwait(false))
                        {
                            await dl.Content.CopyToAsync(fs, ct).ConfigureAwait(false);
                        _log.LogInformation("Downloaded Google font {Family}/{Variant} (arabic: {Arabic})",
                            family, variant, arabic);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogWarning(ex, "Font download {Family}/{Variant} failed", family, variant);
                    }
                }
            }
        }
    }

    private async Task DownloadViaGithubMirrorAsync(HttpClient http, CancellationToken ct)
    {
        foreach (var (id, paths) in GithubMirrorFiles)
        {
            try
            {
                var targetDir = Path.Combine(_dir, id);
                if (CollectFiles(targetDir).Count > 0) continue;
                Directory.CreateDirectory(targetDir);
                var any = false;
                foreach (var repoPath in paths)
                {
                    var url = "https://raw.githubusercontent.com/google/fonts/main/" + repoPath;
                    var dest = Path.Combine(targetDir, Path.GetFileName(repoPath));
                    if (File.Exists(dest)) { any = true; continue; }
                    using var res = await http.GetAsync(url, ct).ConfigureAwait(false);
                    if (!res.IsSuccessStatusCode)
                    {
                        _log.LogWarning("Font download {Id} failed: {Status}", id, res.StatusCode);
                        continue;
                    }
                    var fs = File.Create(dest);
                    await using (fs.ConfigureAwait(false))
                    {
                        await res.Content.CopyToAsync(fs, ct).ConfigureAwait(false);
                    any = true;
                    }
                }
                if (any) _log.LogInformation("Downloaded Google font {Id}", id);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Font download {Id} failed (offline ok)", id);
            }
        }
    }

    private void RefreshFontconfig()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "fc-cache",
                Arguments = $"-f \"{_dir}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = System.Diagnostics.Process.Start(psi);
            p?.WaitForExit(15000);
        }
        catch (Exception ex)
        {
            _log.LogDebug(ex, "fc-cache not available, skipping");
        }
    }

    private List<(string Id, string DisplayName, string Family, bool SupportsArabic)> ReadManifest()
    {
        var defaults = new List<(string, string, string, bool)>
        {
            ("inter", "Inter", "Inter", false),
            ("cairo", "Cairo", "Cairo", true),
            ("ibm-plex-sans-arabic", "IBM Plex Sans Arabic", "IBM Plex Sans Arabic", true),
            ("arial", "Arial", "Arial", false),
            ("times-new-roman", "Times New Roman", "Times New Roman", false),
            ("noto-sans", "Noto Sans", "Noto Sans", false),
            ("noto-serif", "Noto Serif", "Noto Serif", false),
            ("arimo", "Arimo", "Arimo", false),
            ("tinos", "Tinos", "Tinos", false),
            ("roboto", "Roboto", "Roboto", false),
        };
        var path = Path.Combine(_dir, "manifest.json");
        if (!File.Exists(path)) return defaults;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var out_ = new List<(string, string, string, bool)>();
            foreach (var el in doc.RootElement.EnumerateArray())
            {
                var id = NormalizeId(el.GetProperty("id").GetString() ?? "");
                if (string.IsNullOrEmpty(id)) continue;
                var family = el.TryGetProperty("family", out var f) ? f.GetString() ?? ToDisplay(id) : ToDisplay(id);
                var display = el.TryGetProperty("displayName", out var d) ? d.GetString() ?? family : family;
                var ar = el.TryGetProperty("supportsArabic", out var a) && a.GetBoolean();
                out_.Add((id, display, family, ar || GuessArabic(id + family)));
            }
            return out_.Count > 0 ? out_ : defaults;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Bad fonts/manifest.json, using defaults");
            return defaults;
        }
    }

    private static bool GuessArabic(string s)
    {
        s = s.ToLowerInvariant();
        return s.Contains("arab") || s.Contains("cairo") || s.Contains("amiri")
            || s.Contains("naskh") || s.Contains("tajawal") || s.Contains("almarai")
            || s.Contains("naqsh") || s.Contains("aref");
    }

    private static string ToDisplay(string id) =>
        string.Join(' ', id.Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpperInvariant(w[0]) + (w.Length > 1 ? w[1..] : "")));

    private static FontDefinition DefaultDejaVu() =>
        new("dejavu", "DejaVu Sans", "DejaVu Sans", "builtin", false, Array.Empty<string>());
}
