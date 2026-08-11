using supClient.Models;

namespace supClient.Services;

public static class AppThemeService
{
    public static Task ApplyThemeAsync(AppThemePreference theme)
    {
        var requestedTheme = theme == AppThemePreference.Light
            ? AppTheme.Light
            : AppTheme.Dark;

        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (Application.Current is not null)
                Application.Current.UserAppTheme = requestedTheme;
        });
    }
}
