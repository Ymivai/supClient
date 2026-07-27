namespace supClient.Models;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime StartTime { get; set; }

    public TimeSpan Duration { get; set; } = Defines.DefaultBookingDuration;

    public int BoardsCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime EndTime => StartTime + Duration;
}
