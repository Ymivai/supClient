using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using supClient.Messages;
using supClient.Services;
using supClient.Views;

namespace supClient.ViewModels;

public class BookingsPageViewModel : ViewModelBase
{
    readonly IBookingService _bookingService;
    readonly INavigationService _navigationService;
    readonly ILogger<BookingsPageViewModel> _logger;

    DateTime _selectedDate = DateTime.Today;
    string _dateDisplay = string.Empty;
    string _boardUsageDisplay = string.Empty;
    BookingItemViewModel? _selectedBooking;
    int _totalBoards;
    int _occupiedBoards;
    int _availableBoards;
    int _loadVersion;

    public BookingsPageViewModel(
        IBookingService bookingService,
        INavigationService navigationService,
        ILogger<BookingsPageViewModel> logger)
    {
        _bookingService = bookingService;
        _navigationService = navigationService;
        _logger = logger;

        Bookings = new ObservableCollection<BookingItemViewModel>();
        AddBookingCommand = new Command(async () => await AddBookingAsync());
        RefreshCommand = new Command(async () => await LoadBookingsAsync());
        PreviousDayCommand = new Command(async () => await ChangeDateAsync(-1));
        NextDayCommand = new Command(async () => await ChangeDateAsync(1));
        TodayCommand = new Command(async () => await SetDateAsync(DateTime.Today));

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
            var date = value.Date;
            if (SetProperty(ref _selectedDate, date))
            {
                UpdateDateDisplay();
                MainThread.BeginInvokeOnMainThread(async () => await LoadBookingsAsync());
            }
        }
    }

    public string DateDisplay
    {
        get => _dateDisplay;
        private set => SetProperty(ref _dateDisplay, value);
    }

    public int TotalBoards
    {
        get => _totalBoards;
        private set => SetProperty(ref _totalBoards, value);
    }

    public int OccupiedBoards
    {
        get => _occupiedBoards;
        private set => SetProperty(ref _occupiedBoards, value);
    }

    public int AvailableBoards
    {
        get => _availableBoards;
        private set => SetProperty(ref _availableBoards, value);
    }

    public string BoardUsageDisplay
    {
        get => _boardUsageDisplay;
        private set => SetProperty(ref _boardUsageDisplay, value);
    }

    public BookingItemViewModel? SelectedBooking
    {
        get => _selectedBooking;
        set
        {
            if (SetProperty(ref _selectedBooking, value) && value is not null)
                MainThread.BeginInvokeOnMainThread(async () => await EditBookingAsync(value.Id));
        }
    }

    public ICommand AddBookingCommand { get; }

    public ICommand RefreshCommand { get; }

    public ICommand PreviousDayCommand { get; }

    public ICommand NextDayCommand { get; }

    public ICommand TodayCommand { get; }

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
        var loadVersion = ++_loadVersion;

        try
        {
            var selectedDate = SelectedDate;
            var bookings = await _bookingService.GetBookingsByDateAsync(selectedDate);
            var referenceTime = GetReferenceTime(selectedDate);
            var boardUsage = await _bookingService.GetBoardUsageAsync(selectedDate, referenceTime);

            if (loadVersion != _loadVersion)
                return;

            Bookings.Clear();
            foreach (var booking in bookings.OrderBy(b => b.StartTime))
            {
                Bookings.Add(new BookingItemViewModel(booking));
            }

            TotalBoards = boardUsage.TotalBoards;
            OccupiedBoards = boardUsage.OccupiedBoards;
            AvailableBoards = boardUsage.AvailableBoards;
            BoardUsageDisplay = $"Свободно: {AvailableBoards} | Занято: {OccupiedBoards} | Всего: {TotalBoards}";
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

    async Task EditBookingAsync(Guid bookingId)
    {
        if (bookingId == Guid.Empty)
            return;

        await _navigationService.NavigateToPage<AddBookingPage>(bookingId);
        SelectedBooking = null;
    }

    async Task ChangeDateAsync(int days)
    {
        await SetDateAsync(SelectedDate.AddDays(days));
    }

    async Task SetDateAsync(DateTime date)
    {
        var normalizedDate = date.Date;
        if (SelectedDate == normalizedDate)
        {
            await LoadBookingsAsync();
            return;
        }

        SelectedDate = normalizedDate;
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

    static TimeSpan GetReferenceTime(DateTime selectedDate)
        => selectedDate.Date == DateTime.Today ? DateTime.Now.TimeOfDay : TimeSpan.Zero;
}
