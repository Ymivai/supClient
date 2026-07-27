using CommunityToolkit.Mvvm.Messaging.Messages;

namespace supClient.Messages;

public class BookingsChangedMessage : ValueChangedMessage<DateTime>
{
    public BookingsChangedMessage(DateTime date) : base(date)
    {
    }
}
