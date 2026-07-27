using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using supClient.Messages;
using supClient.Services;
using supClient.Storage;
using supClient.Views;

namespace supClient.ViewModels;

public class BookingsPageViewModel : ViewModelBase
{
    readonly IBookingRepository _bookingRepository;
    readonly INavigationService _navigationService;
    readonly ILogger<BookingsPageViewModel> _logger;

    DateTime _selectedDate = DateTime.Today;
    string _dateDisplay = string.Empty;

    public BookingsPageViewModel(
        IBookingRepository bookingRepository,
        INavigationService navigationService,
        ILogger<BookingsPageViewModel> logger)
    {
        _bookingRepository = bookingRepository;
        _navigationService = navigationService;
        _logger = logger;

        Bookings = new ObservableCollection<BookingItemViewModel>();
        AddBookingCommand = new Command(async () => await AddBookingAsync());

        UpdateDateDisplay();
        WeakReferenceMessenger.Default.Register<BookingsChangedMessage>(this, OnBookingsChanged);
        WeakReferenceMessenger.Default.Register<SettingsChangedMessage>(this, OnSettingsChanged);
    }

    public ObservableCollection<BookingItemViewModel> Bookings { get; }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                UpdateDateDisplay();
            }
        }
    }

    public string DateDisplay
    {
        get => _dateDisplay;
        private set => SetProperty(ref _dateDisplay, value);
    }

    public ICommand AddBookingCommand { get; }

    public override async Task OnNavigatedTo()
    {
        await LoadBookingsAsync();
    }

    public override async Task OnReturnedTo()
    {
        await LoadBookingsAsync();
    }

    async Task LoadBookingsAsync()
    {
        try
        {
            var bookings = await _bookingRepository.GetBookingsByDateAsync(SelectedDate);

            Bookings.Clear();
            foreach (var booking in bookings)
            {
                Bookings.Add(new BookingItemViewModel(
                    booking.StartTime,
                    booking.EndTime,
                    booking.BoardsCount));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load bookings for {Date}", SelectedDate);
        }
    }

    async Task AddBookingAsync()
    {
        await _navigationService.NavigateToPage<AddBookingPage>(SelectedDate);
    }

    void OnBookingsChanged(object recipient, BookingsChangedMessage message)
    {
        if (message.Value.Date != SelectedDate.Date)
            return;

        MainThread.BeginInvokeOnMainThread(async () => await LoadBookingsAsync());
    }

    void OnSettingsChanged(object recipient, SettingsChangedMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () => await LoadBookingsAsync());
    }

    void UpdateDateDisplay()
    {
        DateDisplay = SelectedDate.ToString("dddd, d MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));
    }
}
