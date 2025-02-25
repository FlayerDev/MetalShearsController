using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia.Controls.Documents;
using MetalShearsController.Models;

namespace MetalShearsController.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public StepperMotorService? TerminalMotorService;


    #region ObservableProperties&Commands
    [ObservableProperty]
    private string status = "Σύστημα Ανενεργό";

    [ObservableProperty]
    private string statusColor = "DimGray";

    [ObservableProperty]
    private string terminalPosition = "0000.0mm";
    [ObservableProperty]
    private double? requestedTermPos = 0.0;
    [ObservableProperty]
    private bool enableMoveTermBttn = true;
    public ICommand MoveTermAxisCommand { get; }
    #endregion

    public MainViewModel()
    {
        MoveTermAxisCommand = new RelayCommand(MoveTermAxis);
        MemoryAddCommand = new RelayCommand(MemoryAdd);
        MemoryClearCommand = new RelayCommand(MemoryClear);
        MemoryForwardCommand = new RelayCommand(MemoryForward);
    }

    #region MemoryList
    private int _selectedIndex = -1;

    public ObservableCollection<PositionUnits> SavedItems { get; } = new();

    public ICommand MemoryAddCommand { get; }
    public ICommand MemoryClearCommand { get; }
    public ICommand MemoryForwardCommand { get; }

    [ObservableProperty]
    public bool autoRotateTermPos = false;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _selectedIndex = value;
            OnPropertyChanged();
        }
    }
    private void MemoryAdd() { if (SavedItems.Count < 5) SavedItems.Add(new(RequestedTermPos ?? 0.0)); }
    private void MemoryClear() => SavedItems.Clear();

    private void MemoryForward()
    {
        if (SavedItems.Count < 2) return;
        if (SelectedIndex == -1)
        {
            SelectedIndex = 0;
        }
        else if (SelectedIndex == SavedItems.Count - 1)
        {
            SelectedIndex = 0;
        }
        else SelectedIndex++;
    }

    #endregion


    private void MoveTermAxis()
    {
        TerminalPosition = RequestedTermPos.ToString() + " mm";
    }
}
