using CommunityToolkit.Mvvm.ComponentModel;

namespace MetalShearsController.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    [ObservableProperty]
    private string status = "Σύστημα Ανενεργό"; 

    [ObservableProperty]
    private string statusColor = "DimGray";    
    
    [ObservableProperty]
    private string requestedTerminalPosition = "0000.0mm";
}
