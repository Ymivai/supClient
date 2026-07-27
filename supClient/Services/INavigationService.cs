namespace supClient.Services;

public interface INavigationService
{
    Task NavigateToPage<T>(object? parameter = null, bool animated = true) where T : Page;

    Task NavigateBack(bool animated = true);
}
