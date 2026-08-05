using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicPlayer.ViewModels;

public class BaseViewModel : INotifyPropertyChanged {
    //event for property changes
    public event PropertyChangedEventHandler? PropertyChanged;

    //notify UI that property value changed
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}