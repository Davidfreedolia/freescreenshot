using System.Text.Json;
using System.Text.Json.Serialization;

namespace FreeScreenshot.Core.Config;

/// <summary>
/// Persistent settings stored at %LOCALAPPDATA%\FreeScreenshot\config.json.
/// All fields are nullable so a missing or partially-written file degrades
/// gracefully into defaults.
/// </summary>
public sealed class AppConfig
{
    [JsonPropertyName("install_id")]
    public string? InstallId { get; set; }

    /// <summary>User opted in to anonymous install / uninstall telemetry.</summary>
    [JsonPropertyName("tracking_opted_in")]
    public bool TrackingOptedIn { get; set; } = true;

    /// <summary>Email linked from the landing page (if the user pasted the install code).</summary>
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    /// <summary>Version the user explicitly chose to skip on the update prompt.</summary>
    [JsonPropertyName("skipped_update_version")]
    public string? SkippedUpdateVersion { get; set; }

    [JsonPropertyName("consented_privacy_version")]
    public string? ConsentedPrivacyVersion { get; set; }

    [JsonPropertyName("lang")]
    public string? Lang { get; set; }

    public static string ConfigDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FreeScreenshot");

    public static string ConfigPath => Path.Combine(ConfigDirectory, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static AppConfig Load()
    {
        try
        {
            if (!File.Exists(ConfigPath)) return new AppConfig();
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
        }
        catch
        {
            // Corrupt file — start clean rather than crash on launch.
            return new AppConfig();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(ConfigDirectory);
        var json = JsonSerializer.Serialize(this, JsonOptions);
        File.WriteAllText(ConfigPath, json);
    }

    /// <summary>Returns the install_id, generating and persisting one if missing.</summary>
    public string EnsureInstallId()
    {
        if (string.IsNullOrWhiteSpace(InstallId))
        {
            InstallId = Guid.NewGuid().ToString("D");
            Save();
        }
        return InstallId!;
    }
}
