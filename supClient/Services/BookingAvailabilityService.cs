using supClient.Localization;
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
        var settings = await _settingsService.GetSettingsAsync();
        var bookingDuration = duration ?? settings.DefaultBookingDuration;

        if (settings.TotalBoards <= 0)
            return CreateUnavailableResult(settings.TotalBoards, boardsCount, Text("Availability.TotalBoardsInvalid"));

        if (boardsCount <= 0)
            return CreateUnavailableResult(settings.TotalBoards, boardsCount, Text("Availability.BoardsRequired"));

        if (bookingDuration <= TimeSpan.Zero)
            return CreateUnavailableResult(settings.TotalBoards, boardsCount, Text("Availability.DurationRequired"));

        var dayBookings = await _bookingRepository.GetBookingsByDateAsync(startTime.Date);
        var relevantBookings = FilterBookings(dayBookings, excludeBookingId);

        var occupiedBoards = GetOccupiedBoardsAt(startTime, bookingDuration, relevantBookings);
        var availableBoards = GetAvailableBoardsAt(startTime, bookingDuration, relevantBookings, settings.TotalBoards);
        var isAvailable = availableBoards >= boardsCount;
        var conflicts = GetConflictingBookings(startTime, bookingDuration, relevantBookings);

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
            TotalBoards = settings.TotalBoards,
            RequestedBoards = boardsCount,
            AvailableBoards = availableBoards,
            OccupiedBoards = occupiedBoards,
            ConflictingBookings = conflicts,
            NextAvailableStart = nextAvailable
        };
    }

    public int GetOccupiedBoardsAt(
        DateTime startTime,
        TimeSpan duration,
        IReadOnlyList<Booking> bookings)
    {
        var endTime = startTime + duration;
        return GetPeakOccupancy(startTime, endTime, bookings);
    }

    public int GetAvailableBoardsAt(
        DateTime startTime,
        TimeSpan duration,
        IReadOnlyList<Booking> bookings,
        int totalBoards)
    {
        var peakOccupancy = GetOccupiedBoardsAt(startTime, duration, bookings);
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

    static IReadOnlyList<BookingConflict> GetConflictingBookings(
        DateTime startTime,
        TimeSpan duration,
        IReadOnlyList<Booking> bookings)
    {
        var endTime = startTime + duration;

        return bookings
            .Where(b => b.StartTime < endTime && b.EndTime > startTime)
            .Select(b => new BookingConflict
            {
                BookingId = b.Id,
                ClientName = b.ClientName,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                BoardsCount = b.BoardsCount
            })
            .ToList();
    }

    static AvailabilityCheckResult CreateUnavailableResult(int totalBoards, int requestedBoards, string message)
        => new()
        {
            IsAvailable = false,
            TotalBoards = Math.Max(0, totalBoards),
            RequestedBoards = Math.Max(0, requestedBoards),
            AvailableBoards = Math.Max(0, totalBoards),
            OccupiedBoards = 0,
            Message = message
        };

    static string Text(string key)
        => LocalizedResources.Instance[key];
}
