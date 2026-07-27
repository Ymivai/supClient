using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace supClient.Localization;

public class LocalizedResources : INotifyPropertyChanged
{
    static readonly CultureInfo DefaultCultureInfo;
    static readonly ResourceManager ResourceManager;

    CultureInfo _currentCultureInfo = new("uk");

    public static LocalizedResources Instance { get; } = new();

    public CultureInfo CurrentCultureInfo
    {
        get => _currentCultureInfo;
        set
        {
            if (_currentCultureInfo.Equals(value))
                return;

            _currentCultureInfo = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }

    public string this[string key]
        => ResourceManager.GetString(key, CurrentCultureInfo) ?? key;

    static LocalizedResources()
    {
        DefaultCultureInfo = CultureInfo.CurrentUICulture;
        ResourceManager = new ResourceManager("supClient.Resources.Resx.AppResources", Assembly.GetExecutingAssembly());
    }

    protected LocalizedResources()
    {
        CurrentCultureInfo = NormalizeCulture(DefaultCultureInfo);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    static CultureInfo NormalizeCulture(CultureInfo cultureInfo)
        => cultureInfo.TwoLetterISOLanguageName == "ru"
            ? new CultureInfo("ru")
            : new CultureInfo("uk");
}
