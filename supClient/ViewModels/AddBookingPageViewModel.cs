using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using supClient.Localization;
using supClient.Messages;
using supClient.Models;
using supClient.Services;
using supClient.Storage;

namespace supClient.ViewModels;

public class AddBookingPageViewModel : ViewModelBase
{
    readonly IBookingService _bookingService;
    readonly IAppSettingsService _settingsService;
    readonly INavigationService _navigationService;
    readonly IDialogService _dialogService;
    readonly ILogger<AddBookingPageViewModel> _logger;

    Guid? _editingBookingId;
    DateTime? _originalBookingDate;
    DateTime _bookingDate = DateTime.Today;
    TimeSpan _startTime = DateTime.Now.TimeOfDay;
    TimeSpan _endTime = DateTime.Now.TimeOfDay.Add(Defines.DefaultBookingDuration);
    int _boardsCount = 1;
    string _clientName = string.Empty;
    string _phoneNumber = string.Empty;
    string _comment = string.Empty;
    int _selectedPaymentMethodIndex;
    int _svoParticipantsCount;
    bool _hasSvoParticipants;
    string _pageTitle = Text("Title.NewBooking");
    string _durationDisplay = string.Empty;
    string _boardsCountDisplay = string.Empty;
    string _svoParticipantsDisplay = string.Empty;
    bool _isEditing;

    public AddBookingPageViewModel(
        IBookingService bookingService,
        IAppSettingsService settingsService,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<AddBookingPageViewModel> logger)
    {
        _bookingService = bookingService;
        _settingsService = settingsService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _logger = logger;

        PaymentMethodNames = new ObservableCollection<string>();
        RefreshLocalizedTexts();
        LocalizedResources.Instance.PropertyChanged += (_, _) => RefreshLocalizedTexts();

        SaveCommand = new Command(async () => await SaveAsync(), CanSave);
        DeleteCommand = new Command(async () => await DeleteAsync(), () => IsEditing);
        CancelCommand = new Command(async () => await CancelAsync());
    }

    public ObservableCollection<string> PaymentMethodNames { get; }

    public string PageTitle
    {
        get => _pageTitle;
        private set => SetProperty(ref _pageTitle, value);
    }

    public bool IsEditing
    {
        get => _isEditing;
        private set
        {
            if (SetProperty(ref _isEditing, value))
                (DeleteCommand as Command)?.ChangeCanExecute();
        }
    }

    public DateTime BookingDate
    {
        get => _bookingDate;
        set => SetProperty(ref _bookingDate, value.Date);
    }

    public TimeSpan StartTime
    {
        get => _startTime;
        set
        {
            if (SetProperty(ref _startTime, value))
                OnTimeChanged();
        }
    }

    public TimeSpan EndTime
    {
        get => _endTime;
        set
        {
            if (SetProperty(ref _endTime, value))
                OnTimeChanged();
        }
    }

    public string DurationDisplay
    {
        get => _durationDisplay;
        private set => SetProperty(ref _durationDisplay, value);
    }

    public string BoardsCountDisplay
    {
        get => _boardsCountDisplay;
        private set => SetProperty(ref _boardsCountDisplay, value);
    }

    public string SvoParticipantsDisplay
    {
        get => _svoParticipantsDisplay;
        private set => SetProperty(ref _svoParticipantsDisplay, value);
    }

    public int BoardsCount
    {
        get => _boardsCount;
        set
        {
            if (SetProperty(ref _boardsCount, value))
            {
                if (SvoParticipantsCount > BoardsCount)
                    SvoParticipantsCount = BoardsCount;

                UpdateCountDisplays();
                (SaveCommand as Command)?.ChangeCanExecute();
            }
        }
    }

    public string ClientName
    {
        get => _clientName;
        set
        {
            if (SetProperty(ref _clientName, value))
                (SaveCommand as Command)?.ChangeCanExecute();
        }
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set => SetProperty(ref _phoneNumber, value);
    }

    public string Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value);
    }

    public int SelectedPaymentMethodIndex
    {
        get => _selectedPaymentMethodIndex;
        set
        {
            if (SetProperty(ref _selectedPaymentMethodIndex, value))
                (SaveCommand as Command)?.ChangeCanExecute();
        }
    }

    public int SvoParticipantsCount
    {
        get => _svoParticipantsCount;
        set
        {
            var normalizedValue = Math.Clamp(value, 0, BoardsCount);
            if (SetProperty(ref _svoParticipantsCount, normalizedValue))
            {
                UpdateCountDisplays();
                (SaveCommand as Command)?.ChangeCanExecute();
            }
        }
    }

    public bool HasSvoParticipants
    {
        get => _hasSvoParticipants;
        set
        {
            if (SetProperty(ref _hasSvoParticipants, value))
            {
                SvoParticipantsCount = value ? Math.Max(1, SvoParticipantsCount) : 0;
                (SaveCommand as Command)?.ChangeCanExecute();
            }
        }
    }

    public ICommand SaveCommand { get; }

    public ICommand DeleteCommand { get; }

    public ICommand CancelCommand { get; }

    public override async Task OnNavigatingTo(object? parameter)
    {
        if (parameter is Guid bookingId)
        {
            await LoadBookingAsync(bookingId);
            return;
        }

        PageTitle = Text("Title.NewBooking");
        IsEditing = false;
        _editingBookingId = null;
        _originalBookingDate = null;

        if (parameter is DateTime date)
            BookingDate = date.Date;

        var settings = await _settingsService.GetSettingsAsync();
        EndTime = GetDefaultEndTime(StartTime, settings.DefaultBookingDuration);
    }

    async Task LoadBookingAsync(Guid bookingId)
    {
        var booking = await _bookingService.GetBookingByIdAsync(bookingId);
        if (booking is null)
        {
            await _dialogService.DisplayAlertAsync(Text("Dialog.ErrorTitle"), Text("Dialog.BookingNotFound"));
            await _navigationService.NavigateBack();
            return;
        }

        PageTitle = Text("Title.EditBooking");
        IsEditing = true;
        _editingBookingId = booking.Id;
        _originalBookingDate = booking.StartTime.Date;
        BookingDate = booking.StartTime.Date;
        StartTime = booking.StartTime.TimeOfDay;
        EndTime = booking.EndTime.TimeOfDay;
        BoardsCount = booking.BoardsCount;
        SvoParticipantsCount = booking.SvoParticipantsCount;
        HasSvoParticipants = booking.SvoParticipantsCount > 0;
        ClientName = booking.ClientName;
        PhoneNumber = booking.PhoneNumber ?? string.Empty;
        Comment = booking.Comment ?? string.Empty;
        SelectedPaymentMethodIndex = booking.PaymentMethod.ToSelectionIndex();
        UpdateDurationDisplay();
    }

    async Task SaveAsync()
    {
        try
        {
            var now = DateTime.Now;
            var booking = new Booking
            {
                Id = _editingBookingId ?? Guid.NewGuid(),
                StartTime = BookingDate.Date.Add(StartTime),
                Duration = GetDuration(),
                BoardsCount = BoardsCount,
                SvoParticipantsCount = HasSvoParticipants ? SvoParticipantsCount : 0,
                ClientName = ClientName.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                Comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment.Trim(),
                PaymentMethod = PaymentMethodExtensions.FromSelectionIndex(SelectedPaymentMethodIndex),
                CreatedAt = now,
                UpdatedAt = now
            };

            var result = _editingBookingId.HasValue
                ? await _bookingService.UpdateBookingAsync(booking)
                : await _bookingService.CreateBookingAsync(booking);

            if (!result.IsSuccess)
            {
                await _dialogService.DisplayAlertAsync(Text("Dialog.SaveBlockedTitle"), result.ErrorMessage);
                return;
            }

            if (_originalBookingDate.HasValue && _originalBookingDate.Value.Date != BookingDate.Date)
                WeakReferenceMessenger.Default.Send(new BookingsChangedMessage(_originalBookingDate.Value));

            WeakReferenceMessenger.Default.Send(new BookingsChangedMessage(BookingDate));
            await NavigateBackAfterSuccessfulChangeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save booking");
            await _dialogService.DisplayAlertAsync(Text("Dialog.ErrorTitle"), Text("Dialog.SaveBookingFailed"));
        }
    }

    async Task DeleteAsync()
    {
        if (!_editingBookingId.HasValue)
            return;

        try
        {
            var confirmed = await _dialogService.DisplayConfirmationAsync(
                Text("Dialog.DeleteBookingTitle"),
                Text("Dialog.DeleteBookingMessage"),
                Text("Dialog.Delete"),
                Text("Button.Cancel"));

            if (!confirmed)
                return;

            await _bookingService.DeleteBookingAsync(_editingBookingId.Value);
            WeakReferenceMessenger.Default.Send(new BookingsChangedMessage(_originalBookingDate ?? BookingDate));
            await NavigateBackAfterSuccessfulChangeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete booking {BookingId}", _editingBookingId);
            await _dialogService.DisplayAlertAsync(Text("Dialog.ErrorTitle"), Text("Dialog.DeleteBookingFailed"));
        }
    }

    async Task CancelAsync()
    {
        await _navigationService.NavigateBack();
    }

    bool CanSave()
        => BoardsCount > 0
           && (!HasSvoParticipants || SvoParticipantsCount > 0)
           && SvoParticipantsCount <= BoardsCount
           && GetDuration() > TimeSpan.Zero
           && !string.IsNullOrWhiteSpace(ClientName);

    TimeSpan GetDuration()
        => EndTime - StartTime;

    static TimeSpan GetDefaultEndTime(TimeSpan startTime, TimeSpan defaultDuration)
    {
        var endTime = startTime.Add(defaultDuration);
        return endTime >= TimeSpan.FromDays(1)
            ? new TimeSpan(23, 59, 0)
            : endTime;
    }

    void OnTimeChanged()
    {
        UpdateDurationDisplay();
        (SaveCommand as Command)?.ChangeCanExecute();
    }

    void UpdateDurationDisplay()
    {
        var duration = GetDuration();
        if (duration <= TimeSpan.Zero)
        {
            DurationDisplay = Text("Validation.EndAfterStart");
            return;
        }

        DurationDisplay = duration.Hours > 0
            ? string.Format(Text("Format.DurationHoursMinutes"), duration.Hours, duration.Minutes)
            : string.Format(Text("Format.DurationMinutes"), duration.Minutes);
    }

    void RefreshLocalizedTexts()
    {
        RefreshPaymentMethodNames();
        PageTitle = IsEditing ? Text("Title.EditBooking") : Text("Title.NewBooking");
        UpdateDurationDisplay();
        UpdateCountDisplays();
    }

    void RefreshPaymentMethodNames()
    {
        PaymentMethodNames.Clear();
        foreach (var method in PaymentMethodExtensions.BookingPaymentMethods)
        {
            PaymentMethodNames.Add(method.ToDisplayName());
        }

        RaisePropertyChanged(nameof(SelectedPaymentMethodIndex));
    }

    void UpdateCountDisplays()
    {
        BoardsCountDisplay = string.Format(Text("Label.SelectedBoards"), BoardsCount);
        SvoParticipantsDisplay = string.Format(Text("Label.SvoParticipantsSelected"), SvoParticipantsCount);
    }

    async Task NavigateBackAfterSuccessfulChangeAsync()
    {
        try
        {
            await _navigationService.NavigateBack();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Booking was changed but navigation back failed");
        }
    }

    static string Text(string key)
        => LocalizedResources.Instance[key];

    PaymentMethod SelectedPaymentMethod
        => PaymentMethodExtensions.FromSelectionIndex(SelectedPaymentMethodIndex);
}
