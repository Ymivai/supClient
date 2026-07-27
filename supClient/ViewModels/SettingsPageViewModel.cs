using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using supClient.Messages;
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
    int _defaultBookingDurationHours = (int)Defines.DefaultBookingDuration.TotalHours;

    public SettingsPageViewModel(
        IAppSettingsService settingsService,
        IDialogService dialogService,
        ILogger<SettingsPageViewModel> logger)
    {
        _settingsService = settingsService;
        _dialogService = dialogService;
        _logger = logger;

        SaveCommand = new Command(async () => await SaveAsync(), CanSave);
    }

    public int TotalBoards
    {
        get => _totalBoards;
        set
        {
            if (SetProperty(ref _totalBoards, value))
                (SaveCommand as Command)?.ChangeCanExecute();
        }
    }

    public int DefaultBookingDurationHours
    {
        get => _defaultBookingDurationHours;
        set
        {
            if (SetProperty(ref _defaultBookingDurationHours, value))
                (SaveCommand as Command)?.ChangeCanExecute();
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
            DefaultBookingDurationHours = Math.Max(1, (int)settings.DefaultBookingDuration.TotalHours);
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
            var settings = new AppSettings
            {
                TotalBoards = TotalBoards,
                DefaultBookingDuration = TimeSpan.FromHours(DefaultBookingDurationHours)
            };

            await _settingsService.SaveSettingsAsync(settings);
            WeakReferenceMessenger.Default.Send(new SettingsChangedMessage(settings));

            await _dialogService.DisplayAlertAsync("Сохранено", "Настройки успешно сохранены.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            await _dialogService.DisplayAlertAsync("Ошибка", "Не удалось сохранить настройки.");
        }
    }

    bool CanSave()
        => TotalBoards > 0 && DefaultBookingDurationHours > 0;
}
