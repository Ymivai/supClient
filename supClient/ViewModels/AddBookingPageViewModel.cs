using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
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
    int _durationHours = (int)Defines.DefaultBookingDuration.TotalHours;
    int _boardsCount = 1;
    string _clientName = string.Empty;
    string _phoneNumber = string.Empty;
    string _comment = string.Empty;
    int _selectedPaymentMethodIndex;
    string _pageTitle = "Новая бронь";
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

        PaymentMethodNames = Enum.GetValues<PaymentMethod>()
            .OrderBy(m => m.ToSelectionIndex())
            .Select(m => m.ToDisplayName())
            .ToList();

        SaveCommand = new Command(async () => await SaveAsync(), CanSave);
        DeleteCommand = new Command(async () => await DeleteAsync(), () => IsEditing);
        CancelCommand = new Command(async () => await CancelAsync());
    }

    public IReadOnlyList<string> PaymentMethodNames { get; }

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
        set => SetProperty(ref _startTime, value);
    }

    public int DurationHours
    {
        get => _durationHours;
        set
        {
            if (SetProperty(ref _durationHours, value))
                (SaveCommand as Command)?.ChangeCanExecute();
        }
    }

    public int BoardsCount
    {
        get => _boardsCount;
        set
        {
            if (SetProperty(ref _boardsCount, value))
                (SaveCommand as Command)?.ChangeCanExecute();
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
        set => SetProperty(ref _selectedPaymentMethodIndex, value);
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

        PageTitle = "Новая бронь";
        IsEditing = false;
        _editingBookingId = null;
        _originalBookingDate = null;

        if (parameter is DateTime date)
            BookingDate = date.Date;

        var settings = await _settingsService.GetSettingsAsync();
        DurationHours = Math.Max(1, (int)settings.DefaultBookingDuration.TotalHours);
    }

    async Task LoadBookingAsync(Guid bookingId)
    {
        var booking = await _bookingService.GetBookingByIdAsync(bookingId);
        if (booking is null)
        {
            await _dialogService.DisplayAlertAsync("Ошибка", "Бронь не найдена.");
            await _navigationService.NavigateBack();
            return;
        }

        PageTitle = "Редактировать бронь";
        IsEditing = true;
        _editingBookingId = booking.Id;
        _originalBookingDate = booking.StartTime.Date;
        BookingDate = booking.StartTime.Date;
        StartTime = booking.StartTime.TimeOfDay;
        DurationHours = Math.Max(1, (int)booking.Duration.TotalHours);
        BoardsCount = booking.BoardsCount;
        ClientName = booking.ClientName;
        PhoneNumber = booking.PhoneNumber ?? string.Empty;
        Comment = booking.Comment ?? string.Empty;
        SelectedPaymentMethodIndex = booking.PaymentMethod.ToSelectionIndex();
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
                Duration = TimeSpan.FromHours(DurationHours),
                BoardsCount = BoardsCount,
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
                await _dialogService.DisplayAlertAsync("Невозможно сохранить", result.ErrorMessage);
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
            await _dialogService.DisplayAlertAsync("Ошибка", "Не удалось сохранить бронирование.");
        }
    }

    async Task DeleteAsync()
    {
        if (!_editingBookingId.HasValue)
            return;

        try
        {
            var confirmed = await _dialogService.DisplayConfirmationAsync(
                "Удалить бронь?",
                "Это действие удалит выбранную бронь без возможности отмены.",
                "Удалить",
                "Отмена");

            if (!confirmed)
                return;

            await _bookingService.DeleteBookingAsync(_editingBookingId.Value);
            WeakReferenceMessenger.Default.Send(new BookingsChangedMessage(_originalBookingDate ?? BookingDate));
            await NavigateBackAfterSuccessfulChangeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete booking {BookingId}", _editingBookingId);
            await _dialogService.DisplayAlertAsync("Ошибка", "Не удалось удалить бронирование.");
        }
    }

    async Task CancelAsync()
    {
        await _navigationService.NavigateBack();
    }

    bool CanSave()
        => BoardsCount > 0
           && DurationHours > 0
           && !string.IsNullOrWhiteSpace(ClientName);

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

}
