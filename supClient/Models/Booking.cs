namespace supClient.Models;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime StartTime { get; set; }

    public TimeSpan Duration { get; set; } = Defines.DefaultBookingDuration;

    public int BoardsCount { get; set; }

    public int SvoParticipantsCount { get; set; }

    public bool FullHourlyPricing { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Comment { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Unpaid;

    public int CardPaymentAmount { get; set; }

    public int CashPaymentAmount { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Scheduled;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public DateTime EndTime => StartTime + Duration;
}
