using supClient.ViewModels;

namespace supClient.Services;

public class NavigationService : INavigationService
{
    readonly IServiceProvider _services;

    static INavigation? Navigation =>
        Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task NavigateToPage<T>(object? parameter = null, bool animated = true) where T : Page
    {
        var page = _services.GetRequiredService<T>();

        if (page.BindingContext is ViewModelBase navigatingVm)
            await navigatingVm.OnNavigatingTo(parameter);

        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Navigation!.PushAsync(page, animated));

        if (page.BindingContext is ViewModelBase navigatedVm)
            await navigatedVm.OnNavigatedTo();
    }

    public async Task NavigateBack(bool animated = true)
    {
        if (Navigation?.NavigationStack.Count <= 1)
            return;

        var currentPage = Navigation!.NavigationStack[^1];

        if (currentPage.BindingContext is ViewModelBase closingVm)
            await closingVm.OnClosing();

        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Navigation.PopAsync(animated));

        var returnedPage = Navigation.NavigationStack[^1];
        if (returnedPage.BindingContext is ViewModelBase returnedVm)
            await returnedVm.OnReturnedTo();
    }
}
