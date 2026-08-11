using supClient.Models;

namespace supClient.Services;

public static class BookingRevenueCalculator
{
    const int RevenueRoundingStep = 50;
    const int RoundDownThreshold = 20;

    public static int CalculateBookingTotal(Booking booking, int hourlyRate)
    {
        var hours = booking.FullHourlyPricing
            ? Math.Ceiling(booking.Duration.TotalHours)
            : booking.Duration.TotalHours;
        var amount = booking.BoardsCount * (decimal)hours * hourlyRate;

        if (!booking.FullHourlyPricing)
            amount -= CalculateSvoDiscount(booking, hourlyRate);

        var rawAmount = (int)Math.Round(amount, MidpointRounding.AwayFromZero);

        return RoundBookingTotal(rawAmount);
    }

    static int CalculateSvoDiscount(Booking booking, int hourlyRate)
        => Math.Max(0, booking.SvoParticipantsCount) * hourlyRate;

    public static int RoundBookingTotal(int amount)
    {
        if (amount <= 0)
            return 0;

        var lowerBoundary = amount / RevenueRoundingStep * RevenueRoundingStep;
        var differenceFromLowerBoundary = amount - lowerBoundary;

        return differenceFromLowerBoundary <= RoundDownThreshold
            ? lowerBoundary
            : lowerBoundary + RevenueRoundingStep;
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
