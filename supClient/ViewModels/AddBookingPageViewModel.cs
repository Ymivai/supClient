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
    readonly IBookingRepository _bookingRepository;
    readonly IBookingAvailabilityService _availabilityService;
    readonly INavigationService _navigationService;
    readonly IDialogService _dialogService;
    readonly ILogger<AddBookingPageViewModel> _logger;

    DateTime _bookingDate = DateTime.Today;
    TimeSpan _startTime = DateTime.Now.TimeOfDay;
    int _boardsCount = 1;

    public AddBookingPageViewModel(
        IBookingRepository bookingRepository,
        IBookingAvailabilityService availabilityService,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<AddBookingPageViewModel> logger)
    {
        _bookingRepository = bookingRepository;
        _availabilityService = availabilityService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _logger = logger;

        SaveCommand = new Command(async () => await SaveAsync(), () => BoardsCount > 0);
        CancelCommand = new Command(async () => await CancelAsync());
    }

    public DateTime BookingDate
    {
        get => _bookingDate;
        private set => SetProperty(ref _bookingDate, value);
    }

    public TimeSpan StartTime
    {
        get => _startTime;
        set => SetProperty(ref _startTime, value);
    }

    public int BoardsCount
    {
        get => _boardsCount;
        set
        {
            if (SetProperty(ref _boardsCount, value))
            {
                (SaveCommand as Command)?.ChangeCanExecute();
            }
        }
    }

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public override Task OnNavigatingTo(object? parameter)
    {
        if (parameter is DateTime date)
            BookingDate = date.Date;

        return Task.CompletedTask;
    }

    async Task SaveAsync()
    {
        try
        {
            var startDateTime = BookingDate.Date.Add(StartTime);
            var result = await _availabilityService.CheckAvailabilityAsync(startDateTime, BoardsCount);

            if (!result.IsAvailable)
            {
                await _dialogService.DisplayAlertAsync("Невозможно сохранить", result.GetUnavailableMessage());
                return;
            }

            var booking = new Booking
            {
                StartTime = startDateTime,
                Duration = Defines.DefaultBookingDuration,
                BoardsCount = BoardsCount,
                CreatedAt = DateTime.Now
            };

            await _bookingRepository.AddBookingAsync(booking);
            WeakReferenceMessenger.Default.Send(new BookingsChangedMessage(BookingDate));

            await _navigationService.NavigateBack();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save booking");
            await _dialogService.DisplayAlertAsync("Ошибка", "Не удалось сохранить бронирование.");
        }
    }

    async Task CancelAsync()
    {
        await _navigationService.NavigateBack();
    }
}
