namespace supClient.Services;

public class AvailabilityCheckResult
{
    public bool IsAvailable { get; init; }

    public int RequestedBoards { get; init; }

    public int AvailableBoards { get; init; }

    public DateTime? NextAvailableStart { get; init; }

    public string GetUnavailableMessage()
    {
        if (IsAvailable)
            return string.Empty;

        var lines = new List<string>
        {
            "Недостаточно свободных SUP.",
            $"Запрошено: {RequestedBoards}",
            $"Свободно: {AvailableBoards}"
        };

        if (NextAvailableStart.HasValue)
        {
            var needMore = RequestedBoards - AvailableBoards;
            lines.Add($"Ещё {needMore} SUP освободятся в {NextAvailableStart.Value:HH:mm}.");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
