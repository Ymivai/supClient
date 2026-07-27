namespace supClient.Services;

public class DialogService : IDialogService
{
    public Task DisplayAlertAsync(string title, string message, string cancel = "OK")
    {
        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null)
            return Task.CompletedTask;

        return MainThread.InvokeOnMainThreadAsync(() =>
            page.DisplayAlert(title, message, cancel));
    }
}
