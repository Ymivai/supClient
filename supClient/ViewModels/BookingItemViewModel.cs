namespace supClient.ViewModels;

public class BookingItemViewModel
{
    public BookingItemViewModel(DateTime startTime, DateTime endTime, int boardsCount)
    {
        StartTime = startTime;
        EndTime = endTime;
        BoardsCount = boardsCount;
        DisplayText = $"{startTime:HH:mm}–{endTime:HH:mm} | {boardsCount} SUP";
    }

    public DateTime StartTime { get; }

    public DateTime EndTime { get; }

    public int BoardsCount { get; }

    public string DisplayText { get; }
}
