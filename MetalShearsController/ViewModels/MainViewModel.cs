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
using MetalShearsController.Controllers;

namespace MetalShearsController.ViewModels;

public partial class MainViewModel : ViewModelBase
{

#region Constants


    private const int THICKNESS_STEP_PIN = 0;
    private const int THICKNESS_DIR_PIN = 0;    
    private const int THICKNESS_ENABLE_PIN = 0;
#endregion



    private bool isSimulation = false;

    #region ObservableProperties&Commands
    [ObservableProperty]
    private string status = "Σύστημα Ανενεργό";
    [ObservableProperty]
    private string statusColor = "DimGray";

    [ObservableProperty]
    private PositionUnits terminalPosition = new(0.0);
    [ObservableProperty]
    private int[] thicknessPresets = [100, 200];
    
    [ObservableProperty]
    private double? requestedTermPos = 0.0;
    [ObservableProperty]
    private bool enableMoveTermBttn = true;
    public ICommand MoveTermAxisCommand { get; }
    public ICommand SetThicknessAxisCommand { get; }
    public ICommand MoveThicknessAxisCommand { get; }
    #endregion

    public MainViewModel()
    {
        try
        {
            TerminalController.Initialize();
        }
        catch
        {
            isSimulation = true;
            Debug.Print("No GPIO System. Simulation only!");
            //StatusColor = "Red";
            Status = "Non GPIO System (Simulation Mode)";
        }

        MoveTermAxisCommand = new RelayCommand(MoveTermAxis);
        SetThicknessAxisCommand = new RelayCommand<string?>(SetThicknessFromPreset);
        MoveThicknessAxisCommand = new RelayCommand(MoveToSetThickness);
        MemoryAddCommand = new RelayCommand(MemoryAdd);
        MemoryClearCommand = new RelayCommand(MemoryClear);
        MemoryForwardCommand = new RelayCommand(MemoryForward);

        TerminalController.PositionChanged += () => TerminalPosition = new(TerminalController.TerminalPosition);
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


    #region UI Actions
    private void SetThicknessFromPreset(string? preset)
    {
        int preset_index = int.Parse(preset ?? "0");
        Debug.Print($"Set thickness to {preset}");

    }

    #endregion


    #region Real Actions
    private void MoveTermAxis()
    {
        if (isSimulation)
        {
            TerminalController.SetPosition(new PositionUnits(RequestedTermPos ?? 0.0));
            return;
        }
        TerminalController.Translate(new PositionUnits(RequestedTermPos ?? 0.0));
    }


    private void MoveToSetThickness()
    {

    }
}
#endregion