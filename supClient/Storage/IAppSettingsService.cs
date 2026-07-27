using supClient.Models;

namespace supClient.Storage;

public interface IAppSettingsService
{
    Task<AppSettings> GetSettingsAsync();

    Task SaveSettingsAsync(AppSettings settings);
}
