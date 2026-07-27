using supClient.Models;

namespace supClient.Services;

public interface IBookingService
{
    Task<IReadOnlyList<Booking>> GetBookingsByDateAsync(DateTime date);

    Task<Booking?> GetBookingByIdAsync(Guid id);

    Task<BookingSaveResult> CreateBookingAsync(Booking booking);

    Task<BookingSaveResult> UpdateBookingAsync(Booking booking);

    Task DeleteBookingAsync(Guid id);
}
