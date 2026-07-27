using supClient.ViewModels;

namespace supClient.Services;

public class NavigationService : INavigationService
{
    readonly IServiceProvider _services;

    static INavigation? Navigation =>
        Shell.Current?.Navigation
        ?? Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation;

    public NavigationService(IServiceProvider services)
    {
        _services = services;
    }

    public async Task NavigateToPage<T>(object? parameter = null, bool animated = true) where T : Page
    {
        var page = _services.GetRequiredService<T>();
        var navigation = Navigation;

        if (navigation is null)
            return;

        if (page.BindingContext is ViewModelBase navigatingVm)
            await navigatingVm.OnNavigatingTo(parameter);

        await MainThread.InvokeOnMainThreadAsync(async () =>
            await navigation.PushAsync(page, animated));

        if (page.BindingContext is ViewModelBase navigatedVm)
            await navigatedVm.OnNavigatedTo();
    }

    public async Task NavigateBack(bool animated = true)
    {
        var navigation = Navigation;

        if (navigation is null || navigation.NavigationStack.Count <= 1)
            return;

        var currentPage = navigation.NavigationStack[^1];
        var returnedPage = navigation.NavigationStack[^2];

        if (currentPage.BindingContext is ViewModelBase closingVm)
            await closingVm.OnClosing();

        await MainThread.InvokeOnMainThreadAsync(async () =>
            await navigation.PopAsync(animated));

        if (returnedPage?.BindingContext is ViewModelBase returnedVm)
            await returnedVm.OnReturnedTo();
    }
}
