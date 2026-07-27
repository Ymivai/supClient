using supClient.Models;

namespace supClient.ViewModels;

public class BookingItemViewModel
{
    public BookingItemViewModel(Booking booking)
    {
        Id = booking.Id;
        StartTime = booking.StartTime;
        EndTime = booking.EndTime;
        BoardsCount = booking.BoardsCount;
        ClientName = booking.ClientName;
        PhoneNumber = booking.PhoneNumber ?? string.Empty;
        Comment = booking.Comment ?? string.Empty;
        PaymentMethod = GetPaymentMethodDisplay(booking.PaymentMethod);
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

    public string TimeDisplay { get; }

    public string BoardsDisplay { get; }

    public string ClientDisplay { get; }

    public string DetailsDisplay { get; }

    string BuildDetailsDisplay()
    {
        var parts = new List<string> { BoardsDisplay, PaymentMethod };

        if (!string.IsNullOrWhiteSpace(PhoneNumber))
            parts.Add(PhoneNumber);

        if (!string.IsNullOrWhiteSpace(Comment))
            parts.Add(Comment);

        return string.Join(" | ", parts);
    }

    static string GetPaymentMethodDisplay(PaymentMethod paymentMethod)
        => paymentMethod switch
        {
            Models.PaymentMethod.Cash => "Наличные",
            Models.PaymentMethod.Card => "Карта",
            Models.PaymentMethod.Transfer => "Перевод",
            Models.PaymentMethod.Other => "Другое",
            _ => "Не оплачено"
        };
}
