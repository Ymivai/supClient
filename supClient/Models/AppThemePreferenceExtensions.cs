using supClient.Localization;

namespace supClient.Models;

public static class AppThemePreferenceExtensions
{
    public static IReadOnlyList<AppThemePreference> ThemePreferences { get; } =
    [
        AppThemePreference.Light,
        AppThemePreference.Dark
    ];

    public static string ToDisplayName(this AppThemePreference theme)
        => theme switch
        {
            AppThemePreference.Light => LocalizedResources.Instance["Theme.Light"],
            _ => LocalizedResources.Instance["Theme.Dark"]
        };

    public static int ToSelectionIndex(this AppThemePreference theme)
    {
        for (var index = 0; index < ThemePreferences.Count; index++)
        {
            if (ThemePreferences[index] == theme)
                return index;
        }

        return 0;
    }

    public static AppThemePreference FromSelectionIndex(int selectedIndex)
        => selectedIndex >= 0 && selectedIndex < ThemePreferences.Count
            ? ThemePreferences[selectedIndex]
            : AppThemePreference.Dark;
}
