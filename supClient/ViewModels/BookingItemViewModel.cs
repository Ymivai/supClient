using supClient.Models;

namespace supClient.ViewModels;

public class BookingItemViewModel
{
    public BookingItemViewModel(Booking booking, int hourlyRate)
    {
        Id = booking.Id;
        StartTime = booking.StartTime;
        EndTime = booking.EndTime;
        BoardsCount = booking.BoardsCount;
        ClientName = booking.ClientName;
        PhoneNumber = booking.PhoneNumber ?? string.Empty;
        Comment = booking.Comment ?? string.Empty;
        PaymentMethod = booking.PaymentMethod.ToDisplayName();
        Revenue = CalculateRevenue(booking, hourlyRate);
        TimeDisplay = $"{StartTime:HH:mm}-{EndTime:HH:mm}";
        BoardsDisplay = $"{BoardsCount} SUP";
        ClientDisplay = string.IsNullOrWhiteSpace(ClientName) ? "Клиент не указан" : ClientName;
        DetailsDisplay = BuildDetailsDisplay();
    }

    public Guid Id { get; }

    public DateTime StartTime { get; }

    public DateTime EndTime { get; }

    public int BoardsCount { get; }

    public string ClientName { get; }

    public string PhoneNumber { get; }

    public string Comment { get; }

    public string PaymentMethod { get; }

    public int Revenue { get; }

    public string TimeDisplay { get; }

    public string BoardsDisplay { get; }

    public string ClientDisplay { get; }

    public string DetailsDisplay { get; }

    string BuildDetailsDisplay()
    {
        var parts = new List<string> { BoardsDisplay, PaymentMethod, $"{Revenue} грн" };

        if (!string.IsNullOrWhiteSpace(PhoneNumber))
            parts.Add(PhoneNumber);

        if (!string.IsNullOrWhiteSpace(Comment))
            parts.Add(Comment);

        return string.Join(" | ", parts);
    }

    static int CalculateRevenue(Booking booking, int hourlyRate)
    {
        if (booking.PaymentMethod == Models.PaymentMethod.Unpaid)
            return 0;

        var amount = booking.BoardsCount * (decimal)booking.Duration.TotalHours * hourlyRate;
        return (int)Math.Round(amount, MidpointRounding.AwayFromZero);
    }
}
