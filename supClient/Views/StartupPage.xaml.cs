namespace supClient.Views;

public partial class StartupPage : ContentPage
{
    readonly Page _nextPage;
    bool _isNavigating;

    public StartupPage(Page nextPage)
    {
        InitializeComponent();
        _nextPage = nextPage;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isNavigating)
            return;

        _isNavigating = true;

        var backgroundAnimation = AnimateBackgroundAsync();
        await Task.Delay(1300);

        if (Window is not null)
            Window.Page = _nextPage;

        await backgroundAnimation;
    }

    async Task AnimateBackgroundAsync()
    {
        BackgroundImage.Scale = 1.03;
        WaveGlow.TranslationY = 64;

        await Task.WhenAll(
            BackgroundImage.ScaleTo(1.1, 1700, Easing.SinInOut),
            BackgroundImage.FadeTo(0.42, 900, Easing.SinInOut),
            WaveGlow.TranslateTo(0, 16, 1300, Easing.SinInOut),
            WaveGlow.FadeTo(0.55, 1300, Easing.SinInOut));
    }
}
