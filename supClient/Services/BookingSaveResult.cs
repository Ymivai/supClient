using supClient.Models;

namespace supClient.Services;

public class BookingSaveResult
{
    public bool IsSuccess { get; init; }

    public Booking? Booking { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public AvailabilityCheckResult? Availability { get; init; }

    public static BookingSaveResult Success(Booking booking)
        => new()
        {
            IsSuccess = true,
            Booking = booking
        };

    public static BookingSaveResult Failure(string errorMessage, AvailabilityCheckResult? availability = null)
        => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Availability = availability
        };
}
