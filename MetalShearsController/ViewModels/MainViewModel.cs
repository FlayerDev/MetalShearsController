using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia.Controls.Documents;

namespace MetalShearsController.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    #region ObservableProperties&Commands
    [ObservableProperty]
    private string status = "Σύστημα Ανενεργό";

    [ObservableProperty]
    private string statusColor = "DimGray";

    [ObservableProperty]
    private string requestedTerminalPosition = "0000.0mm";
    [ObservableProperty]
    private bool enableMoveTermBttn = true;
    public ICommand MoveTermAxisCommand { get; }
    #endregion

    public MainViewModel()
    {
        MoveTermAxisCommand = new RelayCommand(MoveTermAxis);

    }

    #region MemoryList
    private int _selectedIndex = 1;

    public ObservableCollection<string> SavedItems { get; } = new();

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
    private void MemoryAdd()
    {

    }
    private void MemoryClear()
    {

    }

    #endregion


    private void MoveTermAxis()
    {

    }
}
