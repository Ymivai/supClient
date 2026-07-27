using supClient.Views;

namespace supClient;

public partial class AppShell : Shell
{
    public AppShell(BookingsPage bookingsPage, SettingsPage settingsPage)
    {
        InitializeComponent();

        MainTabBar.Items.Add(new ShellContent
        {
            Title = "Брони",
            Content = bookingsPage,
            Route = "BookingsPage"
        });

        MainTabBar.Items.Add(new ShellContent
        {
            Title = "Настройки",
            Content = settingsPage,
            Route = "SettingsPage"
        });

        Routing.RegisterRoute(nameof(AddBookingPage), typeof(AddBookingPage));
    }
}
