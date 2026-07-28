using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using supClient.Localization;
using supClient.Messages;
using supClient.Models;
using supClient.Services;
using supClient.Storage;
using supClient.Views;

namespace supClient.ViewModels;

public class SettingsPageViewModel : ViewModelBase
{
    readonly IAppSettingsService _settingsService;
    readonly IDataResetService _dataResetService;
    readonly IDialogService _dialogService;
    readonly INavigationService _navigationService;
    readonly ILogger<SettingsPageViewModel> _logger;

    int _totalBoards = Defines.DefaultTotalBoards;
    int _weekdayHourlyRate = 300;
    int _weekendHourlyRate = 350;
    string _totalBoardsDisplay = string.Empty;

    public SettingsPageViewModel(
        IAppSettingsService settingsService,
        IDataResetService dataResetService,
        IDialogService dialogService,
        INavigationService navigationService,
        ILogger<SettingsPageViewModel> logger)
    {
        _settingsService = settingsService;
        _dataResetService = dataResetService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _logger = logger;

        UpdateLocalizedDisplays();
        SaveCommand = new Command(async () => await SaveAsync(), CanSave);
        DeleteAllBookingsCommand = new Command(async () => await DeleteAllBookingsAsync());
    }

    public int TotalBoards
    {
        get => _totalBoards;
        set
        {
            if (SetProperty(ref _totalBoards, value))
            {
                UpdateLocalizedDisplays();
                (SaveCommand as Command)?.ChangeCanExecute();
            }
        }
    }

    public string TotalBoardsDisplay
    {
        get => _totalBoardsDisplay;
        private set => SetProperty(ref _totalBoardsDisplay, value);
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
                DefaultBookingDuration = Defines.DefaultBookingDuration,
                WeekdayHourlyRate = WeekdayHourlyRate,
                WeekendHourlyRate = WeekendHourlyRate
            };

            await _settingsService.SaveSettingsAsync(settings);
            WeakReferenceMessenger.Default.Send(new SettingsChangedMessage(settings));

            await _dialogService.DisplayAlertAsync(Text("Dialog.SavedTitle"), Text("Dialog.SettingsSaved"));
            await _navigationService.NavigateToRootPage<BookingsPage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings");
            await _dialogService.DisplayAlertAsync(Text("Dialog.ErrorTitle"), Text("Dialog.SaveSettingsFailed"));
        }
    }

    async Task DeleteAllBookingsAsync()
    {
        try
        {
            var confirmed = await _dialogService.DisplayConfirmationAsync(
                Text("Dialog.DeleteAllBookingsTitle"),
                Text("Dialog.DeleteAllBookingsMessage"),
                Text("Dialog.Delete"),
                Text("Button.Cancel"));

            if (!confirmed)
                return;

            await _dataResetService.DeleteAllBookingsAsync();
            WeakReferenceMessenger.Default.Send(new AllBookingsDeletedMessage(DateTime.Now));
            await _dialogService.DisplayAlertAsync(Text("Dialog.DoneTitle"), Text("Dialog.AllBookingsDeleted"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete all bookings");
            await _dialogService.DisplayAlertAsync(Text("Dialog.ErrorTitle"), Text("Dialog.DeleteAllBookingsFailed"));
        }
    }

    bool CanSave()
        => TotalBoards > 0
           && WeekdayHourlyRate > 0
           && WeekendHourlyRate > 0;

    void UpdateLocalizedDisplays()
    {
        TotalBoardsDisplay = string.Format(Text("Label.TotalBoards"), TotalBoards);
    }

    static string Text(string key)
        => LocalizedResources.Instance[key];
}
