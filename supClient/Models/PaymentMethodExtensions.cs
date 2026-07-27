using supClient.Localization;

namespace supClient.Models;

public static class PaymentMethodExtensions
{
    public static IReadOnlyList<PaymentMethod> BookingPaymentMethods { get; } =
    [
        PaymentMethod.Cash,
        PaymentMethod.Card,
        PaymentMethod.SvoParticipant
    ];

    public static string ToDisplayName(this PaymentMethod paymentMethod)
        => paymentMethod switch
        {
            PaymentMethod.Cash => LocalizedResources.Instance["Payment.Cash"],
            PaymentMethod.Card => LocalizedResources.Instance["Payment.Card"],
            PaymentMethod.SvoParticipant => LocalizedResources.Instance["Payment.SvoParticipant"],
            PaymentMethod.Transfer => LocalizedResources.Instance["Payment.Transfer"],
            PaymentMethod.Other => LocalizedResources.Instance["Payment.Other"],
            _ => LocalizedResources.Instance["Payment.Unpaid"]
        };

    public static int ToSelectionIndex(this PaymentMethod paymentMethod)
    {
        for (var index = 0; index < BookingPaymentMethods.Count; index++)
        {
            if (BookingPaymentMethods[index] == paymentMethod)
                return index;
        }

        return 0;
    }

    public static PaymentMethod FromSelectionIndex(int selectedIndex)
        => selectedIndex >= 0 && selectedIndex < BookingPaymentMethods.Count
            ? BookingPaymentMethods[selectedIndex]
            : PaymentMethod.Cash;
}
