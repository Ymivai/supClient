using supClient.Models;

namespace supClient.Services;

public interface IBookingAvailabilityService
{
    Task<AvailabilityCheckResult> CheckAvailabilityAsync(
        DateTime startTime,
        int boardsCount,
        TimeSpan? duration = null,
        Guid? excludeBookingId = null);

    int GetOccupiedBoardsAt(DateTime startTime, TimeSpan duration, IReadOnlyList<Booking> bookings);

    int GetAvailableBoardsAt(DateTime startTime, TimeSpan duration, IReadOnlyList<Booking> bookings, int totalBoards);

    DateTime? FindNextAvailableStart(
        DateTime desiredStart,
        int boardsNeeded,
        TimeSpan duration,
        IReadOnlyList<Booking> bookings,
        int totalBoards);
}
