using CommunityToolkit.Mvvm.ComponentModel;

namespace MetalShearsController.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [ObservableProperty]
    private string status = "System Offline"; 

    [ObservableProperty]
    private string statusColor = "Gray";
}
