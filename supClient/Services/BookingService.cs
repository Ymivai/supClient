using supClient.Models;
using supClient.Storage;

namespace supClient.Services;

public class BookingService : IBookingService
{
    readonly IBookingRepository _bookingRepository;
    readonly IBookingAvailabilityService _availabilityService;

    public BookingService(
        IBookingRepository bookingRepository,
        IBookingAvailabilityService availabilityService)
    {
        _bookingRepository = bookingRepository;
        _availabilityService = availabilityService;
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

    public async Task<BookingSaveResult> CreateBookingAsync(Booking booking)
    {
        var validationError = ValidateBooking(booking);
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
        var validationError = ValidateBooking(booking);
        if (!string.IsNullOrWhiteSpace(validationError))
            return BookingSaveResult.Failure(validationError);

        var existing = await _bookingRepository.GetBookingByIdAsync(booking.Id);
        if (existing is null)
            return BookingSaveResult.Failure("Booking was not found.");

        var availability = await _availabilityService.CheckAvailabilityAsync(
            booking.StartTime,
            booking.BoardsCount,
            booking.Duration,
            booking.Id);

        if (!availability.IsAvailable)
            return BookingSaveResult.Failure(availability.GetUnavailableMessage(), availability);

        booking.CreatedAt = existing.CreatedAt;
        booking.UpdatedAt = DateTime.Now;

        await _bookingRepository.UpdateBookingAsync(booking);
        return BookingSaveResult.Success(booking);
    }

    public Task DeleteBookingAsync(Guid id)
        => _bookingRepository.DeleteBookingAsync(id);

    static string ValidateBooking(Booking booking)
    {
        if (booking.StartTime == default)
            return "Booking start time is required.";

        if (booking.Duration <= TimeSpan.Zero)
            return "Booking duration must be greater than zero.";

        if (booking.BoardsCount <= 0)
            return "At least one SUP board must be selected.";

        return string.Empty;
    }
}
