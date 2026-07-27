using CommunityToolkit.Mvvm.Messaging.Messages;

namespace supClient.Messages;

public class AllBookingsDeletedMessage : ValueChangedMessage<DateTime>
{
    public AllBookingsDeletedMessage(DateTime deletedAt) : base(deletedAt)
    {
    }
}
