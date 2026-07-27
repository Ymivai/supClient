namespace supClient.Models;

public class BookingConflict
{
    public Guid BookingId { get; init; }

    public string ClientName { get; init; } = string.Empty;

    public DateTime StartTime { get; init; }

    public DateTime EndTime { get; init; }

    public int BoardsCount { get; init; }
}
