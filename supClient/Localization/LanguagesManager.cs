using System.Globalization;

namespace supClient.Localization;

public class LanguagesManager
{
    const string SelectedCultureKey = "SelectedCulture";

    public IReadOnlyList<LanguageModel> SupportedLanguages { get; } =
    [
        new()
        {
            Name = "Language.Russian",
            Culture = new CultureInfo("ru")
        },
        new()
        {
            Name = "Language.Ukrainian",
            Culture = new CultureInfo("uk")
        }
    ];

    public LanguageModel CurrentLanguage { get; private set; }

    public LanguagesManager()
    {
        CurrentLanguage = GetSavedLanguage();
        Apply(CurrentLanguage);
    }

    public void SetLanguage(LanguageModel language)
    {
        CurrentLanguage = language;
        Preferences.Default.Set(SelectedCultureKey, language.Culture.Name);
        Apply(language);
    }

    LanguageModel GetSavedLanguage()
    {
        var selectedCultureName = Preferences.Default.Get(SelectedCultureKey, string.Empty);
        var selectedLanguage = SupportedLanguages.FirstOrDefault(language =>
            language.Culture.Name.Equals(selectedCultureName, StringComparison.OrdinalIgnoreCase));

        if (selectedLanguage is not null)
            return selectedLanguage;

        var currentCulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "uk"
            ? "uk"
            : "ru";

        return SupportedLanguages.First(language => language.Culture.Name == currentCulture);
    }

    static void Apply(LanguageModel language)
    {
        CultureInfo.CurrentCulture = language.Culture;
        CultureInfo.CurrentUICulture = language.Culture;
        LocalizedResources.Instance.CurrentCultureInfo = language.Culture;
    }
}
