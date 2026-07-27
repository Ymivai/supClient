using System.Windows.Input;
using Microsoft.Extensions.Logging;
using supClient.Models;
using supClient.Services;
using supClient.Storage;

namespace supClient.ViewModels;

public class SettingsPageViewModel : ViewModelBase
{
    readonly IAppSettingsService _settingsService;
    readonly IDialogService _dialogService;
    readonly ILogger<SettingsPageViewModel> _logger;

    int _totalBoards = Defines.DefaultTotalBoards;

    public SettingsPageViewModel(
        IAppSettingsService settingsService,
        IDialogService dialogService,
        ILogger<SettingsPageViewModel> logger)
    {
        _settingsService = settingsService;
        _dialogService = dialogService;
        _logger = logger;

        SaveCommand = new Command(async () => await SaveAsync(), () => TotalBoards > 0);
    }

    public int TotalBoards
    {
        get => _totalBoards;
        set
        {
            if (SetProperty(ref _totalBoards, value))
            {
                (SaveCommand as Command)?.ChangeCanExecute();
            }
        }
    }

    public ICommand SaveCommand { get; }

    public override async Task OnNavigatedTo()
    {
        await LoadSettingsAsync();
    }

    async Task LoadSettingsAsync()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            TotalBoards = settings.TotalBoards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load settings");
        }
    }

    async Task SaveAsync()
    {
        try
        {
            await _settingsService.SaveSettingsAsync(new AppSettings
            {
                TotalBoards = TotalBoards
            });

            await _dialogService.DisplayAlertAsync("Сохранено", "Настройки успешно сохранены.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            await _dialogService.DisplayAlertAsync("Ошибка", "Не удалось сохранить настройки.");
        }
    }
}
