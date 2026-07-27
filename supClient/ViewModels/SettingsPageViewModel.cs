using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using supClient.Localization;
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
    readonly LanguagesManager _languagesManager;
    readonly ILogger<SettingsPageViewModel> _logger;

    int _totalBoards = Defines.DefaultTotalBoards;
    int _defaultBookingDurationHours = (int)Defines.DefaultBookingDuration.TotalHours;
    int _weekdayHourlyRate = 300;
    int _weekendHourlyRate = 350;
    int _selectedLanguageIndex;

    public SettingsPageViewModel(
        IAppSettingsService settingsService,
        IDataResetService dataResetService,
        IDialogService dialogService,
        LanguagesManager languagesManager,
        ILogger<SettingsPageViewModel> logger)
    {
        _settingsService = settingsService;
        _dataResetService = dataResetService;
        _dialogService = dialogService;
        _languagesManager = languagesManager;
        _logger = logger;

        LanguageNames = _languagesManager.SupportedLanguages
            .Select(language => Text(language.Name))
            .ToList();
        _selectedLanguageIndex = GetCurrentLanguageIndex();
        SaveCommand = new Command(async () => await SaveAsync(), CanSave);
        DeleteAllBookingsCommand = new Command(async () => await DeleteAllBookingsAsync());
    }

    public IReadOnlyList<string> LanguageNames { get; }

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

    public int SelectedLanguageIndex
    {
        get => _selectedLanguageIndex;
        set
        {
            if (SetProperty(ref _selectedLanguageIndex, value))
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
            SelectedLanguageIndex = GetCurrentLanguageIndex();
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
            ApplySelectedLanguage();
            WeakReferenceMessenger.Default.Send(new SettingsChangedMessage(settings));

            await _dialogService.DisplayAlertAsync(Text("Dialog.SavedTitle"), Text("Dialog.SettingsSaved"));
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
           && DefaultBookingDurationHours > 0
           && WeekdayHourlyRate > 0
           && WeekendHourlyRate > 0
           && SelectedLanguageIndex >= 0
           && SelectedLanguageIndex < _languagesManager.SupportedLanguages.Count;

    int GetCurrentLanguageIndex()
    {
        var index = _languagesManager.SupportedLanguages
            .Select((language, i) => new { language, i })
            .FirstOrDefault(item => item.language.Culture.Name == _languagesManager.CurrentLanguage.Culture.Name)
            ?.i;

        return index ?? 0;
    }

    void ApplySelectedLanguage()
    {
        if (SelectedLanguageIndex < 0 || SelectedLanguageIndex >= _languagesManager.SupportedLanguages.Count)
            return;

        _languagesManager.SetLanguage(_languagesManager.SupportedLanguages[SelectedLanguageIndex]);
    }

    static string Text(string key)
        => LocalizedResources.Instance[key];
}
