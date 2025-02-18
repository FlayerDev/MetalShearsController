using Avalonia.Controls;

namespace MetalShearsController.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.WindowState = WindowState.FullScreen;
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        //this.ShowInTaskbar = false;
        this.CanResize = false;
    }
}