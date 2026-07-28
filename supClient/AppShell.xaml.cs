using supClient.Localization;
using supClient.Views;

namespace supClient;

public partial class AppShell : Shell
{
    readonly ShellContent _bookingsShellContent;
    readonly ShellContent _settingsShellContent;

    public AppShell(BookingsPage bookingsPage, SettingsPage settingsPage, LanguagesManager languagesManager)
    {
        InitializeComponent();

        _bookingsShellContent = new ShellContent
        {
            Title = Text("Title.Bookings"),
            Content = bookingsPage,
            Route = nameof(BookingsPage)
        };

        _settingsShellContent = new ShellContent
        {
            Title = Text("Title.Settings"),
            Content = settingsPage,
            Route = nameof(SettingsPage)
        };

        MainTabBar.Items.Add(_bookingsShellContent);
        MainTabBar.Items.Add(_settingsShellContent);
        LocalizedResources.Instance.PropertyChanged += (_, _) => UpdateLocalizedTitles();

        Routing.RegisterRoute(nameof(AddBookingPage), typeof(AddBookingPage));
    }

    void UpdateLocalizedTitles()
    {
        Title = Text("App.Title");
        _bookingsShellContent.Title = Text("Title.Bookings");
        _settingsShellContent.Title = Text("Title.Settings");
    }

    static string Text(string key)
        => LocalizedResources.Instance[key];
}
