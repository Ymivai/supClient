using System.Text.Json;
using supClient.Models;

namespace supClient.Storage;

public class AppSettingsService : IAppSettingsService
{
    readonly string _filePath;
    readonly SemaphoreSlim _lock = new(1, 1);

    static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AppSettingsService()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, Defines.SettingsFileName);
    }

    public async Task<AppSettings> GetSettingsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(_filePath))
                return new AppSettings();

            await using var stream = File.OpenRead(_filePath);
            var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, JsonOptions);
            return settings ?? new AppSettings();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        await _lock.WaitAsync();
        try
        {
            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions);
        }
        finally
        {
            _lock.Release();
        }
    }
}
