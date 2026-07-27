namespace supClient.Models;

public class BookingDuration
{
    public BookingDurationKind Kind { get; set; } = BookingDurationKind.TwoHours;

    public TimeSpan? CustomDuration { get; set; }

    public bool IsOpenEnded => Kind == BookingDurationKind.OpenEnded;

    public TimeSpan ToTimeSpan()
    {
        return Kind switch
        {
            BookingDurationKind.OneHour => TimeSpan.FromHours(1),
            BookingDurationKind.TwoHours => TimeSpan.FromHours(2),
            BookingDurationKind.ThreeHours => TimeSpan.FromHours(3),
            BookingDurationKind.Custom => CustomDuration ?? Defines.DefaultBookingDuration,
            BookingDurationKind.OpenEnded => TimeSpan.Zero,
            _ => Defines.DefaultBookingDuration
        };
    }
}

public enum BookingDurationKind
{
    OneHour = 1,
    TwoHours = 2,
    ThreeHours = 3,
    Custom = 4,
    OpenEnded = 5
}
