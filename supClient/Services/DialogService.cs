using supClient.Localization;

namespace supClient.Services;

public class DialogService : IDialogService
{
    public Task DisplayAlertAsync(string title, string message, string cancel = "")
    {
        cancel = string.IsNullOrWhiteSpace(cancel)
            ? LocalizedResources.Instance["Dialog.Ok"]
            : cancel;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null)
            return Task.CompletedTask;

        return MainThread.InvokeOnMainThreadAsync(() =>
            page.DisplayAlert(title, message, cancel));
    }

    public Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel)
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null)
            return Task.FromResult(false);

        return MainThread.InvokeOnMainThreadAsync(() =>
            page.DisplayAlert(title, message, accept, cancel));
    }
}
