using supClient.Storage;

namespace supClient.Services;

public class DataResetService : IDataResetService
{
    readonly IBookingRepository _bookingRepository;

    public DataResetService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public Task DeleteAllBookingsAsync()
        => _bookingRepository.DeleteAllBookingsAsync();
}
