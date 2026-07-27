using CommunityToolkit.Mvvm.Messaging.Messages;
using supClient.Models;

namespace supClient.Messages;

public class SettingsChangedMessage : ValueChangedMessage<AppSettings>
{
    public SettingsChangedMessage(AppSettings settings) : base(settings)
    {
    }
}
