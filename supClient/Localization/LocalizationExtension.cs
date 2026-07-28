using System.Globalization;

namespace supClient.Localization;

[ContentProperty(nameof(Key))]
[AcceptEmptyServiceProvider]
public class LocalizationExtension : IMarkupExtension
{
    public string? Key { get; set; }

    public object ProvideValue(IServiceProvider? serviceProvider)
    {
        if (Key is null)
            return string.Empty;

        var provideValueTarget = serviceProvider?.GetService<IProvideValueTarget>();
        if (provideValueTarget?.TargetProperty is System.Reflection.PropertyInfo propertyInfo
            && propertyInfo.Name == "StringFormat")
        {
            return LocalizedResources.Instance[Key];
        }

        return new Binding(
            nameof(LocalizedResources.CurrentCultureInfo),
            source: LocalizedResources.Instance,
            converter: LocalizationValueConverter.Instance,
            converterParameter: Key);
    }
}

public class LocalizationValueConverter : IValueConverter
{
    public static LocalizationValueConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => parameter is string key
            ? LocalizedResources.Instance[key]
            : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
