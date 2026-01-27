namespace TimeTracker.App.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;


public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _applicationTitle = "TimeTracker";
}
