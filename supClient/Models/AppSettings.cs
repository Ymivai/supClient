namespace supClient.Models;

public class AppSettings
{
    public int TotalBoards { get; set; } = Defines.DefaultTotalBoards;

    public TimeSpan DefaultBookingDuration { get; set; } = Defines.DefaultBookingDuration;

    public int WeekdayHourlyRate { get; set; } = 300;

    public int WeekendHourlyRate { get; set; } = 350;

    public AppThemePreference Theme { get; set; } = AppThemePreference.Dark;
}
