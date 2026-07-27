using supClient.Models;
using supClient.Storage;

namespace supClient.Services;

public class BookingAvailabilityService : IBookingAvailabilityService
{
    readonly IBookingRepository _bookingRepository;
    readonly IAppSettingsService _settingsService;

    public BookingAvailabilityService(
        IBookingRepository bookingRepository,
        IAppSettingsService settingsService)
    {
        _bookingRepository = bookingRepository;
        _settingsService = settingsService;
    }

    public async Task<AvailabilityCheckResult> CheckAvailabilityAsync(
        DateTime startTime,
        int boardsCount,
        TimeSpan? duration = null,
        Guid? excludeBookingId = null)
    {
        var bookingDuration = duration ?? Defines.DefaultBookingDuration;
        var settings = await _settingsService.GetSettingsAsync();
        var dayBookings = await _bookingRepository.GetBookingsByDateAsync(startTime.Date);
        var relevantBookings = FilterBookings(dayBookings, excludeBookingId);

        var availableBoards = GetAvailableBoardsAt(startTime, bookingDuration, relevantBookings, settings.TotalBoards);
        var isAvailable = availableBoards >= boardsCount;

        DateTime? nextAvailable = null;
        if (!isAvailable)
        {
            nextAvailable = FindNextAvailableStart(
                startTime,
                boardsCount,
                bookingDuration,
                relevantBookings,
                settings.TotalBoards);
        }

        return new AvailabilityCheckResult
        {
            IsAvailable = isAvailable,
            RequestedBoards = boardsCount,
            AvailableBoards = availableBoards,
            NextAvailableStart = nextAvailable
        };
    }

    public int GetAvailableBoardsAt(
        DateTime startTime,
        TimeSpan duration,
        IReadOnlyList<Booking> bookings,
        int totalBoards)
    {
        var endTime = startTime + duration;
        var peakOccupancy = GetPeakOccupancy(startTime, endTime, bookings);
        return Math.Max(0, totalBoards - peakOccupancy);
    }

    public DateTime? FindNextAvailableStart(
        DateTime desiredStart,
        int boardsNeeded,
        TimeSpan duration,
        IReadOnlyList<Booking> bookings,
        int totalBoards)
    {
        var candidates = new SortedSet<DateTime> { desiredStart };

        foreach (var booking in bookings)
        {
            if (booking.EndTime >= desiredStart)
                candidates.Add(booking.EndTime);

            if (booking.StartTime >= desiredStart)
                candidates.Add(booking.StartTime);
        }

        foreach (var candidate in candidates)
        {
            if (candidate < desiredStart)
                continue;

            var available = GetAvailableBoardsAt(candidate, duration, bookings, totalBoards);
            if (available >= boardsNeeded)
                return candidate;
        }

        return null;
    }

    static int GetPeakOccupancy(DateTime windowStart, DateTime windowEnd, IReadOnlyList<Booking> bookings)
    {
        var events = new List<(DateTime Time, int Delta)>();

        foreach (var booking in bookings)
        {
            if (booking.StartTime >= windowEnd || booking.EndTime <= windowStart)
                continue;

            var effectiveStart = booking.StartTime > windowStart ? booking.StartTime : windowStart;
            var effectiveEnd = booking.EndTime < windowEnd ? booking.EndTime : windowEnd;

            if (effectiveStart >= effectiveEnd)
                continue;

            events.Add((effectiveStart, booking.BoardsCount));
            events.Add((effectiveEnd, -booking.BoardsCount));
        }

        events.Sort((a, b) =>
        {
            var timeCompare = a.Time.CompareTo(b.Time);
            return timeCompare != 0 ? timeCompare : a.Delta.CompareTo(b.Delta);
        });

        var current = 0;
        var peak = 0;

        foreach (var (_, delta) in events)
        {
            current += delta;
            if (current > peak)
                peak = current;
        }

        return peak;
    }

    static List<Booking> FilterBookings(IReadOnlyList<Booking> bookings, Guid? excludeBookingId)
    {
        if (!excludeBookingId.HasValue)
            return bookings.ToList();

        return bookings.Where(b => b.Id != excludeBookingId.Value).ToList();
    }
}
