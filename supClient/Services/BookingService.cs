using supClient.Localization;
using supClient.Models;
using supClient.Storage;

namespace supClient.Services;

public class BookingService : IBookingService
{
    readonly IBookingRepository _bookingRepository;
    readonly IBookingAvailabilityService _availabilityService;
    readonly IAppSettingsService _settingsService;

    public BookingService(
        IBookingRepository bookingRepository,
        IBookingAvailabilityService availabilityService,
        IAppSettingsService settingsService)
    {
        _bookingRepository = bookingRepository;
        _availabilityService = availabilityService;
        _settingsService = settingsService;
    }

    public async Task<IReadOnlyList<Booking>> GetBookingsByDateAsync(DateTime date)
    {
        var bookings = await _bookingRepository.GetBookingsByDateAsync(date);
        return bookings
            .OrderBy(b => b.StartTime)
            .ToList();
    }

    public Task<Booking?> GetBookingByIdAsync(Guid id)
        => _bookingRepository.GetBookingByIdAsync(id);

    public async Task<BoardUsageResult> GetBoardUsageAsync(DateTime date, TimeSpan referenceTime)
    {
        var settings = await _settingsService.GetSettingsAsync();
        var bookings = await _bookingRepository.GetBookingsByDateAsync(date);
        var referenceDateTime = date.Date.Add(referenceTime);
        var occupiedBoards = bookings
            .Where(b => b.StartTime <= referenceDateTime && b.EndTime > referenceDateTime)
            .Sum(b => b.BoardsCount);
        var availableBoards = Math.Max(0, settings.TotalBoards - occupiedBoards);
        var hourlyRate = GetHourlyRate(date, settings);
        var cardRevenue = bookings.Sum(b => BookingRevenueCalculator.GetCardRevenue(b, hourlyRate));
        var cashRevenue = bookings.Sum(b => BookingRevenueCalculator.GetCashRevenue(b, hourlyRate));
        var totalRevenue = cardRevenue + cashRevenue;

        return new BoardUsageResult
        {
            TotalBoards = settings.TotalBoards,
            OccupiedBoards = occupiedBoards,
            AvailableBoards = availableBoards,
            ReferenceTime = referenceTime,
            HourlyRate = hourlyRate,
            CardRevenue = cardRevenue,
            CashRevenue = cashRevenue,
            TotalRevenue = totalRevenue,
            AdminRevenue = CalculateAdminRevenue(totalRevenue)
        };
    }

    public async Task<BookingSaveResult> CreateBookingAsync(Booking booking)
    {
        var settings = await _settingsService.GetSettingsAsync();
        var validationError = ValidateBooking(booking, settings);
        if (!string.IsNullOrWhiteSpace(validationError))
            return BookingSaveResult.Failure(validationError);

        var availability = await _availabilityService.CheckAvailabilityAsync(
            booking.StartTime,
            booking.BoardsCount,
            booking.Duration);

        if (!availability.IsAvailable)
            return BookingSaveResult.Failure(availability.GetUnavailableMessage(), availability);

        var now = DateTime.Now;
        booking.CreatedAt = booking.CreatedAt == default ? now : booking.CreatedAt;
        booking.UpdatedAt = now;

        await _bookingRepository.AddBookingAsync(booking);
        return BookingSaveResult.Success(booking);
    }

    public async Task<BookingSaveResult> UpdateBookingAsync(Booking booking)
    {
        var settings = await _settingsService.GetSettingsAsync();
        var validationError = ValidateBooking(booking, settings);
        if (!string.IsNullOrWhiteSpace(validationError))
            return BookingSaveResult.Failure(validationError);

        var existing = await _bookingRepository.GetBookingByIdAsync(booking.Id);
        if (existing is null)
            return BookingSaveResult.Failure(Text("Dialog.BookingNotFound"));

        var editedBookingId = existing.Id;
        var availability = await _availabilityService.CheckAvailabilityAsync(
            booking.StartTime,
            booking.BoardsCount,
            booking.Duration,
            editedBookingId);

        if (!availability.IsAvailable)
            return BookingSaveResult.Failure(availability.GetUnavailableMessage(), availability);

        booking.CreatedAt = existing.CreatedAt;
        booking.UpdatedAt = DateTime.Now;

        await _bookingRepository.UpdateBookingAsync(booking);
        return BookingSaveResult.Success(booking);
    }

    public Task DeleteBookingAsync(Guid id)
        => _bookingRepository.DeleteBookingAsync(id);

    static string ValidateBooking(Booking booking, AppSettings settings)
    {
        if (booking.StartTime == default)
            return Text("Validation.StartTimeRequired");

        if (booking.Duration <= TimeSpan.Zero)
            return Text("Validation.DurationRequired");

        if (booking.BoardsCount <= 0)
            return Text("Validation.BoardsRequired");

        if (booking.SvoParticipantsCount < 0 || booking.SvoParticipantsCount > booking.BoardsCount)
            return Text("Validation.SvoParticipantsInvalid");

        if (string.IsNullOrWhiteSpace(booking.ClientName))
            return Text("Validation.CustomerNameRequired");

        if (booking.CardPaymentAmount < 0 || booking.CashPaymentAmount < 0)
            return Text("Validation.PaymentAmountInvalid");

        var paymentTotal = booking.CardPaymentAmount + booking.CashPaymentAmount;
        if (booking.PaymentMethod == PaymentMethod.Unpaid)
        {
            if (paymentTotal != 0)
                return string.Format(Text("Validation.PaymentSplitMismatch"), 0);

            return string.Empty;
        }

        var hourlyRate = GetHourlyRate(booking.StartTime.Date, settings);
        var bookingTotal = BookingRevenueCalculator.CalculateBookingTotal(booking, hourlyRate);
        if (paymentTotal != bookingTotal)
            return string.Format(Text("Validation.PaymentSplitMismatch"), bookingTotal);

        return string.Empty;
    }

    static int GetHourlyRate(DateTime date, AppSettings settings)
        => IsWeekend(date)
            ? settings.WeekendHourlyRate
            : settings.WeekdayHourlyRate;

    static bool IsWeekend(DateTime date)
        => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    static int CalculateAdminRevenue(int totalRevenue)
        => (int)Math.Round(totalRevenue * 0.2m, MidpointRounding.AwayFromZero);

    static string Text(string key)
        => LocalizedResources.Instance[key];
}
