using CommunityToolkit.Mvvm.ComponentModel;

namespace TimeTracker.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _applicationTitle = "TimeTracker";
}
