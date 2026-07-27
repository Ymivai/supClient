using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace supClient.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public virtual Task OnNavigatingTo(object? parameter)
        => Task.CompletedTask;

    public virtual Task OnNavigatedFrom(bool isForwardNavigation)
        => Task.CompletedTask;

    public virtual Task OnNavigatedTo()
        => Task.CompletedTask;

    public virtual Task OnReturnedTo()
        => Task.CompletedTask;

    public virtual Task OnClosing()
        => Task.CompletedTask;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void RaisePropertyChanged([CallerMemberName] string? property = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        storage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
