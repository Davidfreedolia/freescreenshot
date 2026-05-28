using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Freezshot.Core.Config;

namespace Freezshot.Core.Telemetry;

/// <summary>
/// Thin client over the freedolia.com/api/Freezshot/* endpoints.
/// Every method fails silently — telemetry must never crash the app and
/// uninstall pings must never block the uninstaller. All calls respect the
/// user's opt-in flag in <see cref="AppConfig.TrackingOptedIn"/>.
/// </summary>
public sealed class TelemetryClient
{
    private const string BaseUrl = "https://freedolia.com/api/Freezshot";

    // Public anti-spam key. Not a secret (it ships inside the desktop binary)
    // — just enough to keep random callers off the endpoint.
    private const string PublicKey = "Freezshot-public-v1";

    private readonly HttpClient _http;
    private readonly AppConfig _config;

    public TelemetryClient(AppConfig config, HttpClient? http = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        if (!_http.DefaultRequestHeaders.Contains("X-Freezshot-Key"))
        {
            _http.DefaultRequestHeaders.Add("X-Freezshot-Key", PublicKey);
        }
    }

    /// <summary>Ping the backend that this install has just been launched for the first time.</summary>
    public async Task TryReportInstallAsync(string appVersion, string? lang, string? os, CancellationToken ct = default)
    {
        if (!_config.TrackingOptedIn) return;
        try
        {
            var body = new InstallBody
            {
                install_id = _config.EnsureInstallId(),
                email = _config.Email,
                app_version = appVersion,
                lang = lang,
                os = os,
            };
            using var res = await _http.PostAsJsonAsync($"{BaseUrl}/install", body, ct);
            // Discard response — fire-and-forget.
        }
        catch
        {
            // Network or server error — ignore.
        }
    }

    /// <summary>Fetch the currently published version. Returns null on any error.</summary>
    public async Task<LatestResponse?> TryGetLatestAsync(CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<LatestResponse>($"{BaseUrl}/latest", ct);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Best-effort uninstall ping with a short timeout — never blocks.</summary>
    public async Task TryReportUninstallAsync(
        IReadOnlyList<string> reasons,
        string? note,
        string appVersion,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout ?? TimeSpan.FromSeconds(3));

            var body = new UninstallBody
            {
                install_id = _config.EnsureInstallId(),
                reasons = reasons.ToArray(),
                note = note,
                app_version = appVersion,
            };
            using var res = await _http.PostAsJsonAsync($"{BaseUrl}/uninstall", body, cts.Token);
        }
        catch
        {
            // Swallow — the user is uninstalling, we don't get a second chance.
        }
    }

    // ---- DTOs ----

    private sealed class InstallBody
    {
        [JsonPropertyName("install_id")] public string install_id { get; set; } = "";
        [JsonPropertyName("email")]      public string? email      { get; set; }
        [JsonPropertyName("app_version")]public string? app_version{ get; set; }
        [JsonPropertyName("lang")]       public string? lang       { get; set; }
        [JsonPropertyName("os")]         public string? os         { get; set; }
    }

    private sealed class UninstallBody
    {
        [JsonPropertyName("install_id")] public string install_id { get; set; } = "";
        [JsonPropertyName("reasons")]    public string[]? reasons { get; set; }
        [JsonPropertyName("note")]       public string? note       { get; set; }
        [JsonPropertyName("app_version")]public string? app_version{ get; set; }
    }

    public sealed class LatestResponse
    {
        [JsonPropertyName("ok")]            public bool ok            { get; set; }
        [JsonPropertyName("version")]       public string? version    { get; set; }
        [JsonPropertyName("download_url")]  public string? download_url { get; set; }
        [JsonPropertyName("notes")]         public string? notes      { get; set; }
        [JsonPropertyName("updated_at")]    public string? updated_at { get; set; }
    }
}
