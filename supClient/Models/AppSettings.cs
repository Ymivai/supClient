namespace supClient.Models;

public class AppSettings
{
    public int TotalBoards { get; set; } = Defines.DefaultTotalBoards;

    public TimeSpan DefaultBookingDuration { get; set; } = Defines.DefaultBookingDuration;
}
