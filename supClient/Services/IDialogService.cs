namespace supClient.Services;

public interface IDialogService
{
    Task DisplayAlertAsync(string title, string message, string cancel = "OK");

    Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel);
}
