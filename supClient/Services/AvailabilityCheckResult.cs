using supClient.Localization;
using supClient.Models;

namespace supClient.Services;

public class AvailabilityCheckResult
{
    public bool IsAvailable { get; init; }

    public int TotalBoards { get; init; }

    public int RequestedBoards { get; init; }

    public int AvailableBoards { get; init; }

    public int OccupiedBoards { get; init; }

    public int MissingBoards => Math.Max(0, RequestedBoards - AvailableBoards);

    public IReadOnlyList<BookingConflict> ConflictingBookings { get; init; } = [];

    public string Message { get; init; } = string.Empty;

    public DateTime? NextAvailableStart { get; init; }

    public string GetUnavailableMessage()
    {
        if (IsAvailable)
            return string.Empty;

        if (!string.IsNullOrWhiteSpace(Message))
            return Message;

        var lines = new List<string>
        {
            Text("Availability.NotEnoughBoards"),
            string.Format(Text("Availability.TotalBoards"), TotalBoards),
            string.Format(Text("Availability.RequestedBoards"), RequestedBoards),
            string.Format(Text("Availability.OccupiedBoards"), OccupiedBoards),
            string.Format(Text("Availability.AvailableBoards"), AvailableBoards)
        };

        if (NextAvailableStart.HasValue)
            lines.Add(string.Format(Text("Format.NextAvailable"), MissingBoards, NextAvailableStart.Value));

        return string.Join(Environment.NewLine, lines);
    }

    static string Text(string key)
        => LocalizedResources.Instance[key];
}
