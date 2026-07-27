using supClient.Localization;

namespace supClient.Models;

public static class PaymentMethodExtensions
{
    public static string ToDisplayName(this PaymentMethod paymentMethod)
        => paymentMethod switch
        {
            PaymentMethod.Cash => LocalizedResources.Instance["Payment.Cash"],
            PaymentMethod.Card => LocalizedResources.Instance["Payment.Card"],
            PaymentMethod.Transfer => LocalizedResources.Instance["Payment.Transfer"],
            PaymentMethod.Other => LocalizedResources.Instance["Payment.Other"],
            _ => LocalizedResources.Instance["Payment.Unpaid"]
        };

    public static int ToSelectionIndex(this PaymentMethod paymentMethod)
        => paymentMethod switch
        {
            PaymentMethod.Cash => 1,
            PaymentMethod.Card => 2,
            PaymentMethod.Transfer => 3,
            PaymentMethod.Other => 4,
            _ => 0
        };

    public static PaymentMethod FromSelectionIndex(int selectedIndex)
        => selectedIndex switch
        {
            1 => PaymentMethod.Cash,
            2 => PaymentMethod.Card,
            3 => PaymentMethod.Transfer,
            4 => PaymentMethod.Other,
            _ => PaymentMethod.Unpaid
        };
}
