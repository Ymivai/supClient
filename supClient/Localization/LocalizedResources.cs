using System.ComponentModel;
using System.Globalization;
using System.Resources;

namespace supClient.Localization;

public class LocalizedResources : INotifyPropertyChanged
{
    static readonly ResourceManager ResourceManager = new(
        "supClient.Resources.Resx.AppResources",
        typeof(LocalizedResources).Assembly);

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

    protected LocalizedResources()
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;

}
