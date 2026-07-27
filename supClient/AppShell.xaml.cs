using supClient.Localization;
using supClient.Views;

namespace supClient;

public partial class AppShell : Shell
{
    public AppShell(BookingsPage bookingsPage, SettingsPage settingsPage, LanguagesManager languagesManager)
    {
        InitializeComponent();

        MainTabBar.Items.Add(new ShellContent
        {
            Title = Text("Title.Bookings"),
            Content = bookingsPage,
            Route = nameof(BookingsPage)
        });

        MainTabBar.Items.Add(new ShellContent
        {
            Title = Text("Title.Settings"),
            Content = settingsPage,
            Route = nameof(SettingsPage)
        });

        Routing.RegisterRoute(nameof(AddBookingPage), typeof(AddBookingPage));
    }

    static string Text(string key)
        => LocalizedResources.Instance[key];
}
