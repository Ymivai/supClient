using supClient.Localization;
using supClient.Models;
using supClient.Services;

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
        CardPaymentAmount = BookingRevenueCalculator.GetCardRevenue(booking, hourlyRate);
        CashPaymentAmount = BookingRevenueCalculator.GetCashRevenue(booking, hourlyRate);
        IsPaid = CardPaymentAmount + CashPaymentAmount > 0;
        PaymentBorderColor = IsPaid ? Color.FromArgb("#2E7D32") : Color.FromArgb("#D84315");
        PaymentBorderThickness = 2;
        Revenue = CardPaymentAmount + CashPaymentAmount;
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

    public int CardPaymentAmount { get; }

    public int CashPaymentAmount { get; }

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

        if (IsPaid && CardPaymentAmount > 0 && CashPaymentAmount > 0)
        {
            parts.Add(string.Format(Text("Format.CardPaidAmount"), CardPaymentAmount));
            parts.Add(string.Format(Text("Format.CashPaidAmount"), CashPaymentAmount));
        }
        else if (IsPaid)
        {
            parts.Add(PaymentMethod);
        }

        parts.Add(string.Format(Text("Format.BookingRevenue"), Revenue));

        if (SvoParticipantsCount > 0)
            parts.Add(string.Format(Text("Format.SvoParticipants"), SvoParticipantsCount));

        if (!string.IsNullOrWhiteSpace(PhoneNumber))
            parts.Add(PhoneNumber);

        if (!string.IsNullOrWhiteSpace(Comment))
            parts.Add(Comment);

        return string.Join(" | ", parts);
    }

    static string Text(string key)
        => LocalizedResources.Instance[key];
}
