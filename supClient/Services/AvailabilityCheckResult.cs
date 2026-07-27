using supClient.Models;

namespace supClient.Services;

public class AvailabilityCheckResult
{
    public bool IsAvailable { get; init; }

    public int RequestedBoards { get; init; }

    public int AvailableBoards { get; init; }

    public int OccupiedBoards { get; init; }

    public int MissingBoards => Math.Max(0, RequestedBoards - AvailableBoards);

    public IReadOnlyList<BookingConflict> ConflictingBookings { get; init; } = [];

    public DateTime? NextAvailableStart { get; init; }

    public string GetUnavailableMessage()
    {
        if (IsAvailable)
            return string.Empty;

        var lines = new List<string>
        {
            "Недостаточно свободных SUP.",
            $"Запрошено: {RequestedBoards}",
            $"Свободно: {AvailableBoards}",
            $"Занято: {OccupiedBoards}"
        };

        if (NextAvailableStart.HasValue)
        {
            lines.Add($"Ещё {MissingBoards} SUP освободятся в {NextAvailableStart.Value:HH:mm}.");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
