using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace MetalShearsController.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    [ObservableProperty]
    private string status = "Σύστημα Ανενεργό"; 

    [ObservableProperty]
    private string statusColor = "DimGray";    
    
    [ObservableProperty]
    private string requestedTerminalPosition = "0000.0mm";
    private int requestedTerminalPositionUnit = 0;
    public ICommand IncrementRequestedTeminalPositionCommand { get; }

    public MainViewModel()
    {
        IncrementRequestedTeminalPositionCommand = ReactiveCommand.Create(IncrementRequestedTeminalPosition);

    }

    private void IncrementRequestedTeminalPosition ()
    {
        Debug.Print("!!");
        //requestedTerminalPositionUnit += int.Parse(value);
        //RequestedTerminalPosition = (requestedTerminalPositionUnit).ToString() + " mm";
    }
}
