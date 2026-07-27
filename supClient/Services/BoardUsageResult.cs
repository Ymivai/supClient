namespace supClient.Services;

public class BoardUsageResult
{
    public int TotalBoards { get; init; }

    public int OccupiedBoards { get; init; }

    public int AvailableBoards { get; init; }

    public TimeSpan ReferenceTime { get; init; }

    public int HourlyRate { get; init; }

    public int CardRevenue { get; init; }

    public int CashRevenue { get; init; }

    public int TotalRevenue { get; init; }

    public int AdminRevenue { get; init; }
}
