using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using supClient.Localization;
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
    string _availableBoardsDisplay = string.Empty;
    string _cardRevenueDisplay = string.Empty;
    string _cashRevenueDisplay = string.Empty;
    string _totalRevenueDisplay = string.Empty;
    string _adminRevenueDisplay = string.Empty;
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
        WeakReferenceMessenger.Default.Register<AllBookingsDeletedMessage>(this, OnAllBookingsDeleted);
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

    public string AvailableBoardsDisplay
    {
        get => _availableBoardsDisplay;
        private set => SetProperty(ref _availableBoardsDisplay, value);
    }

    public string CardRevenueDisplay
    {
        get => _cardRevenueDisplay;
        private set => SetProperty(ref _cardRevenueDisplay, value);
    }

    public string CashRevenueDisplay
    {
        get => _cashRevenueDisplay;
        private set => SetProperty(ref _cashRevenueDisplay, value);
    }

    public string TotalRevenueDisplay
    {
        get => _totalRevenueDisplay;
        private set => SetProperty(ref _totalRevenueDisplay, value);
    }

    public string AdminRevenueDisplay
    {
        get => _adminRevenueDisplay;
        private set => SetProperty(ref _adminRevenueDisplay, value);
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
                Bookings.Add(new BookingItemViewModel(booking, boardUsage.HourlyRate));
            }

            TotalBoards = boardUsage.TotalBoards;
            OccupiedBoards = boardUsage.OccupiedBoards;
            AvailableBoards = boardUsage.AvailableBoards;
            UpdateSummaryDisplays(boardUsage);
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

    void OnAllBookingsDeleted(object recipient, AllBookingsDeletedMessage message)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            Bookings.Clear();
            OccupiedBoards = 0;
            AvailableBoards = TotalBoards;
            UpdateSummaryDisplays(new BoardUsageResult
            {
                TotalBoards = TotalBoards,
                AvailableBoards = AvailableBoards
            });
            await LoadBookingsAsync();
        });
    }

    void UpdateDateDisplay()
    {
        DateDisplay = SelectedDate.ToString("dddd, d MMMM yyyy", LocalizedResources.Instance.CurrentCultureInfo);
    }

    void UpdateSummaryDisplays(BoardUsageResult boardUsage)
    {
        AvailableBoardsDisplay = string.Format(Text("Format.AvailableBoards"), boardUsage.AvailableBoards);
        CardRevenueDisplay = string.Format(Text("Format.CardRevenue"), boardUsage.CardRevenue);
        CashRevenueDisplay = string.Format(Text("Format.CashRevenue"), boardUsage.CashRevenue);
        TotalRevenueDisplay = string.Format(Text("Format.TotalRevenue"), boardUsage.TotalRevenue);
        AdminRevenueDisplay = string.Format(Text("Format.AdminRevenue"), boardUsage.AdminRevenue);
    }

    static TimeSpan GetReferenceTime(DateTime selectedDate)
        => selectedDate.Date == DateTime.Today ? DateTime.Now.TimeOfDay : TimeSpan.Zero;

    static string Text(string key)
        => LocalizedResources.Instance[key];
}
