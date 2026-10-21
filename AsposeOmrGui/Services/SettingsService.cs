using System;
using System.IO;
using System.Text;
using System.Text.Json;
using AsposeOmrGui.Models;

namespace AsposeOmrGui.Services;

/// <summary>
/// Loads and saves <see cref="AppSettings"/> as JSON under the per-user AppData folder.
/// Writes are atomic (temp file + replace) so a crash mid-save cannot corrupt the store.
/// </summary>
public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string SettingsDirectory { get; }
    public string SettingsFilePath { get; }

    public SettingsService()
    {
        SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AsposeOmrGui");
        SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");
    }

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath, Encoding.UTF8);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
                    if (settings is not null)
                        return Normalize(settings);
                }
            }
        }
        catch
        {
            // A corrupt or unreadable settings file must never prevent the app from starting;
            // fall back to a fresh, empty configuration.
        }

        return new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(SettingsDirectory);
        string json = JsonSerializer.Serialize(settings, JsonOptions);

        string tempPath = SettingsFilePath + ".tmp";
        File.WriteAllText(tempPath, json, new UTF8Encoding(false));

        if (File.Exists(SettingsFilePath))
            File.Replace(tempPath, SettingsFilePath, null);
        else
            File.Move(tempPath, SettingsFilePath);
    }

    /// <summary>Repairs any null collections that an older/partial settings file might carry.</summary>
    private static AppSettings Normalize(AppSettings settings)
    {
        settings.Templates ??= new();
        settings.Cases ??= new();
        foreach (var t in settings.Templates)
            t.Id ??= Guid.NewGuid().ToString("N");
        foreach (var c in settings.Cases)
        {
            c.Id ??= Guid.NewGuid().ToString("N");
            c.FilePaths ??= new();
            c.Answers ??= new();
        }
        return settings;
    }
}
