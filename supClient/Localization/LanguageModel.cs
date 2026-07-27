using System.Globalization;

namespace supClient.Localization;

public class LanguageModel
{
    public string Name { get; init; } = string.Empty;

    public CultureInfo Culture { get; init; } = CultureInfo.CurrentUICulture;
}
