using supClient.Views;

namespace supClient;

public partial class App : Application
{
    readonly AppShell _appShell;

    public App(AppShell appShell)
    {
        InitializeComponent();
        _appShell = appShell;
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new(new StartupPage(_appShell));
}
