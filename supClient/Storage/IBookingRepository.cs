using supClient.Models;

namespace supClient.Storage;

public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetBookingsByDateAsync(DateTime date);

    Task AddBookingAsync(Booking booking);

    Task DeleteBookingAsync(Guid id);

    Task UpdateBookingAsync(Booking booking);
}
