using System.Globalization;

namespace supClient.Localization;

public class LanguagesManager
{
    const string SelectedCultureKey = "SelectedCultureV2";

    public IReadOnlyList<LanguageModel> SupportedLanguages { get; } =
    [
        new()
        {
            Name = "Language.Ukrainian",
            Culture = new CultureInfo("uk")
        },
        new()
        {
            Name = "Language.Russian",
            Culture = new CultureInfo("ru")
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

        return SupportedLanguages.First(language => language.Culture.Name == "uk");
    }

    static void Apply(LanguageModel language)
    {
        CultureInfo.CurrentCulture = language.Culture;
        CultureInfo.CurrentUICulture = language.Culture;
        LocalizedResources.Instance.CurrentCultureInfo = language.Culture;
    }
}
