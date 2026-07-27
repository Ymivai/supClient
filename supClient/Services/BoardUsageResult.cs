namespace supClient.Services;

public class BoardUsageResult
{
    public int TotalBoards { get; init; }

    public int OccupiedBoards { get; init; }

    public int AvailableBoards { get; init; }

    public TimeSpan ReferenceTime { get; init; }
}
