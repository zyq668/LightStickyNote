using System.IO;
using System.Text.Json;
using LightStickyNote.App.Models;

namespace LightStickyNote.App.Services;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _settingsFilePath;

    public SettingsService(string settingsFilePath)
    {
        _settingsFilePath = settingsFilePath;
    }

    public async Task<AppSettings> LoadAsync()
    {
        var directory = Path.GetDirectoryName(_settingsFilePath)!;
        Directory.CreateDirectory(directory);

        if (!File.Exists(_settingsFilePath))
        {
            var defaults = new AppSettings();
            await SaveAsync(defaults);
            return defaults;
        }

        try
        {
            await using var stream = File.OpenRead(_settingsFilePath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, SerializerOptions);
            return settings ?? new AppSettings();
        }
        catch (JsonException)
        {
            var backupPath = $"{_settingsFilePath}.broken-{DateTime.Now:yyyyMMddHHmmss}.json";
            File.Move(_settingsFilePath, backupPath, overwrite: true);

            var defaults = new AppSettings();
            await SaveAsync(defaults);
            return defaults;
        }
    }

    public async Task SaveAsync(AppSettings settings)
    {
        var directory = Path.GetDirectoryName(_settingsFilePath)!;
        Directory.CreateDirectory(directory);
        var tempFilePath = $"{_settingsFilePath}.tmp";

        await using (var stream = File.Create(tempFilePath))
        {
            await JsonSerializer.SerializeAsync(stream, settings, SerializerOptions);
        }

        File.Move(tempFilePath, _settingsFilePath, overwrite: true);
    }
}
