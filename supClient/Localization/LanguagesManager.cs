using System.Globalization;

namespace supClient.Localization;

public class LanguagesManager
{
    static readonly LanguageModel UkrainianLanguage = new()
    {
        Name = "Language.Ukrainian",
        Culture = new CultureInfo("uk")
    };

    public IReadOnlyList<LanguageModel> SupportedLanguages { get; } =
    [
        UkrainianLanguage
    ];

    public LanguageModel CurrentLanguage { get; private set; }

    public LanguagesManager()
    {
        CurrentLanguage = UkrainianLanguage;
        Apply(CurrentLanguage);
    }

    public void SetLanguage(LanguageModel language)
    {
        CurrentLanguage = UkrainianLanguage;
        Apply(CurrentLanguage);
    }

    static void Apply(LanguageModel language)
    {
        CultureInfo.CurrentCulture = language.Culture;
        CultureInfo.CurrentUICulture = language.Culture;
        LocalizedResources.Instance.CurrentCultureInfo = language.Culture;
    }
}
