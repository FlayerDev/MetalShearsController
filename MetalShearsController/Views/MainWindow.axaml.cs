using Avalonia.Controls;

namespace MetalShearsController.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        this.Height = 480.0;
        this.Width = 800.0;
        //this.WindowState = WindowState.FullScreen;
        //this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        //this.ShowInTaskbar = false;
        //this.CanResize = false;
    }
}