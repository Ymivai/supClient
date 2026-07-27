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
    readonly IDataResetService _dataResetService;
    readonly IDialogService _dialogService;
    readonly ILogger<SettingsPageViewModel> _logger;

    int _totalBoards = Defines.DefaultTotalBoards;
    int _defaultBookingDurationHours = (int)Defines.DefaultBookingDuration.TotalHours;
    int _weekdayHourlyRate = 300;
    int _weekendHourlyRate = 350;

    public SettingsPageViewModel(
        IAppSettingsService settingsService,
        IDataResetService dataResetService,
        IDialogService dialogService,
        ILogger<SettingsPageViewModel> logger)
    {
        _settingsService = settingsService;
        _dataResetService = dataResetService;
        _dialogService = dialogService;
        _logger = logger;

        SaveCommand = new Command(async () => await SaveAsync(), CanSave);
        DeleteAllBookingsCommand = new Command(async () => await DeleteAllBookingsAsync());
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

    public int WeekdayHourlyRate
    {
        get => _weekdayHourlyRate;
        set
        {
            if (SetProperty(ref _weekdayHourlyRate, value))
                (SaveCommand as Command)?.ChangeCanExecute();
        }
    }

    public int WeekendHourlyRate
    {
        get => _weekendHourlyRate;
        set
        {
            if (SetProperty(ref _weekendHourlyRate, value))
                (SaveCommand as Command)?.ChangeCanExecute();
        }
    }

    public ICommand SaveCommand { get; }

    public ICommand DeleteAllBookingsCommand { get; }

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
            WeekdayHourlyRate = Math.Max(1, settings.WeekdayHourlyRate);
            WeekendHourlyRate = Math.Max(1, settings.WeekendHourlyRate);
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
                DefaultBookingDuration = TimeSpan.FromHours(DefaultBookingDurationHours),
                WeekdayHourlyRate = WeekdayHourlyRate,
                WeekendHourlyRate = WeekendHourlyRate
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

    async Task DeleteAllBookingsAsync()
    {
        try
        {
            var confirmed = await _dialogService.DisplayConfirmationAsync(
                "Удалить все брони?",
                "Это действие полностью удалит все локально сохраненные бронирования. Настройки останутся без изменений.",
                "Удалить",
                "Отмена");

            if (!confirmed)
                return;

            await _dataResetService.DeleteAllBookingsAsync();
            WeakReferenceMessenger.Default.Send(new AllBookingsDeletedMessage(DateTime.Now));
            await _dialogService.DisplayAlertAsync("Готово", "Все локальные бронирования удалены.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete all bookings");
            await _dialogService.DisplayAlertAsync("Ошибка", "Не удалось удалить локальные бронирования.");
        }
    }

    bool CanSave()
        => TotalBoards > 0
           && DefaultBookingDurationHours > 0
           && WeekdayHourlyRate > 0
           && WeekendHourlyRate > 0;
}
