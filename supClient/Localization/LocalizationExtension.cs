namespace supClient.Localization;

[ContentProperty(nameof(Key))]
[AcceptEmptyServiceProvider]
public class LocalizationExtension : IMarkupExtension
{
    public string? Key { get; set; }

    public object ProvideValue(IServiceProvider serviceProvider)
        => Key is null ? string.Empty : LocalizedResources.Instance[Key];
}
