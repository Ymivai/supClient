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
            "Недостаточно свободных SUP.",
            $"Всего досок: {TotalBoards}",
            $"Запрошено: {RequestedBoards}",
            $"Занято: {OccupiedBoards}",
            $"Свободно: {AvailableBoards}"
        };

        if (NextAvailableStart.HasValue)
        {
            lines.Add($"Ещё {MissingBoards} SUP освободятся в {NextAvailableStart.Value:HH:mm}.");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
