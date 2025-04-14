using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Media;
using MetalShearsController.Models;
using MetalShearsController.Services;

namespace MetalShearsController.Controllers;

public static class TerminalController
{
    private const int TERMINAL_STEP_PIN = 18;
    private const int TERMINAL_DIR_PIN = 24;
    private const int TERMINAL_ENABLE_PIN = 23;
    private const int TERMINAL_UNITS_TO_STEPS = 4;
    private const int TERMINA_MAX_SPEED_STEPS = 20;
    private const bool TERMINAL_INVERT_DIR = false;

    public static PositionUnits TerminalPosition = new PositionUnits(0.0);


    static StepperMotorService? TerminalMotorService;
    public static void Initialize()
    {
        LogService.Log("Initializing Terminal Motor Service");
        TerminalMotorService = new StepperMotorService(TERMINAL_STEP_PIN, TERMINAL_DIR_PIN, TERMINAL_ENABLE_PIN);
        LogService.Log("Terminal Motor Service Initialized");
    }

    public static void Translate(PositionUnits position)
    {
        LogService.Log($"Translating Terminal to {position}");
        int unitDifference = position.Units - TerminalPosition.Units;
        bool dir = unitDifference > 0;
        if (TerminalMotorService != null)
        {
            _ = TerminalMotorService.MoveStepperAsync(unitDifference * TERMINAL_UNITS_TO_STEPS, TERMINA_MAX_SPEED_STEPS, TERMINAL_INVERT_DIR ? !dir : dir);
        }
        else{
            Debug.Print("No Terminal Motor Service, Aborting Translation");
            LogService.Log("No Terminal Motor Service, Aborting Translation");
        }
    }

    public static void SetPosition(PositionUnits position)
    {
        TerminalPosition = position;
        Debug.Print($"Set Term Pos {position}");
        PositionChanged();
    }
    public static Action PositionChanged = () => { };

    public static void Kill()
    {
        TerminalMotorService?.Dispose();
    }

}
