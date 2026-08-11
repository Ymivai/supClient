using supClient.Models;

namespace supClient.Services;

public static class BookingRevenueCalculator
{
    public static int CalculateBookingTotal(Booking booking, int hourlyRate)
    {
        var paidBoardsCount = Math.Max(0, booking.BoardsCount - booking.SvoParticipantsCount);
        var amount = paidBoardsCount * (decimal)booking.Duration.TotalHours * hourlyRate;
        return (int)Math.Round(amount, MidpointRounding.AwayFromZero);
    }

    public static int GetCardRevenue(Booking booking, int hourlyRate)
    {
        if (booking.PaymentMethod is PaymentMethod.Unpaid or PaymentMethod.Transfer or PaymentMethod.Other)
            return 0;

        if (HasManualPaymentAmounts(booking))
            return booking.CardPaymentAmount;

        return booking.PaymentMethod == PaymentMethod.Card
            ? CalculateBookingTotal(booking, hourlyRate)
            : 0;
    }

    public static int GetCashRevenue(Booking booking, int hourlyRate)
    {
        if (booking.PaymentMethod is PaymentMethod.Unpaid or PaymentMethod.Transfer or PaymentMethod.Other)
            return 0;

        if (HasManualPaymentAmounts(booking))
            return booking.CashPaymentAmount;

        return booking.PaymentMethod == PaymentMethod.Cash
            ? CalculateBookingTotal(booking, hourlyRate)
            : 0;
    }

    public static int GetPaidRevenue(Booking booking, int hourlyRate)
        => GetCardRevenue(booking, hourlyRate) + GetCashRevenue(booking, hourlyRate);

    public static bool HasManualPaymentAmounts(Booking booking)
        => booking.CardPaymentAmount > 0 || booking.CashPaymentAmount > 0;
}
