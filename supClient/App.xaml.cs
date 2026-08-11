using supClient.Storage;
using supClient.Services;

namespace supClient;

public partial class App : Application
{
    readonly AppShell _appShell;

    public App(AppShell appShell, IAppSettingsService settingsService)
    {
        InitializeComponent();
        _appShell = appShell;
        _ = ApplySavedThemeAsync(settingsService);
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(_appShell);

    async Task ApplySavedThemeAsync(IAppSettingsService settingsService)
    {
        var settings = await settingsService.GetSettingsAsync();
        await AppThemeService.ApplyThemeAsync(settings.Theme);
    }
}
