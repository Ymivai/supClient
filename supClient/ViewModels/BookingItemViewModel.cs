using supClient.Localization;
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
        SvoParticipantsCount = booking.SvoParticipantsCount;
        ClientName = booking.ClientName;
        PhoneNumber = booking.PhoneNumber ?? string.Empty;
        Comment = booking.Comment ?? string.Empty;
        PaymentMethod = booking.PaymentMethod.ToDisplayName();
        IsPaid = booking.PaymentMethod is Models.PaymentMethod.Cash or Models.PaymentMethod.Card;
        PaymentBorderColor = IsPaid ? Color.FromArgb("#2E7D32") : Color.FromArgb("#D84315");
        PaymentBorderThickness = 2;
        Revenue = CalculateRevenue(booking, hourlyRate);
        TimeDisplay = $"{StartTime:HH:mm}-{EndTime:HH:mm}";
        BoardsDisplay = string.Format(Text("Format.Boards"), BoardsCount);
        ClientDisplay = string.IsNullOrWhiteSpace(ClientName) ? Text("Booking.ClientNotSpecified") : ClientName;
        DetailsDisplay = BuildDetailsDisplay();
    }

    public Guid Id { get; }

    public DateTime StartTime { get; }

    public DateTime EndTime { get; }

    public int BoardsCount { get; }

    public int SvoParticipantsCount { get; }

    public string ClientName { get; }

    public string PhoneNumber { get; }

    public string Comment { get; }

    public string PaymentMethod { get; }

    public bool IsPaid { get; }

    public Color PaymentBorderColor { get; }

    public double PaymentBorderThickness { get; }

    public int Revenue { get; }

    public string TimeDisplay { get; }

    public string BoardsDisplay { get; }

    public string ClientDisplay { get; }

    public string DetailsDisplay { get; }

    string BuildDetailsDisplay()
    {
        var parts = new List<string>
        {
            BoardsDisplay
        };

        if (IsPaid)
            parts.Add(PaymentMethod);

        parts.Add(string.Format(Text("Format.BookingRevenue"), Revenue));

        if (SvoParticipantsCount > 0)
            parts.Add(string.Format(Text("Format.SvoParticipants"), SvoParticipantsCount));

        if (!string.IsNullOrWhiteSpace(PhoneNumber))
            parts.Add(PhoneNumber);

        if (!string.IsNullOrWhiteSpace(Comment))
            parts.Add(Comment);

        return string.Join(" | ", parts);
    }

    static int CalculateRevenue(Booking booking, int hourlyRate)
    {
        if (booking.PaymentMethod is Models.PaymentMethod.Unpaid or Models.PaymentMethod.Transfer or Models.PaymentMethod.Other)
            return 0;

        var paidBoardsCount = Math.Max(0, booking.BoardsCount - booking.SvoParticipantsCount);
        var amount = paidBoardsCount * (decimal)booking.Duration.TotalHours * hourlyRate;
        return (int)Math.Round(amount, MidpointRounding.AwayFromZero);
    }

    static string Text(string key)
        => LocalizedResources.Instance[key];
}
