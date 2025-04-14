using System;
using System.IO;
using System.Diagnostics;

namespace MetalShearsController.Services;
// every time log is called its going to append the new log in file
public class LogService
{
    private static string logFilePath = "log.txt";

    public static Action? LogUpdated;
    public static string LogBuffer
    {
        get => logBuffer;
        set
        {
            logBuffer = value;
            LogUpdated?.Invoke();
        }
    }
    private static string logBuffer = string.Empty;

    public static void Log(string message)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                var logbuff = $"{DateTime.Now:HH:mm:ss} - {message}";
                Debug.Print(logbuff);
                LogBuffer += logbuff + Environment.NewLine;
                writer.WriteLine(logbuff);
            }
        }
        catch (Exception ex)
        {
            Debug.Print($"Failed to write to log: {ex.Message}");
        }
    }

    public static void ClearLog()
    {
        try
        {
            File.WriteAllText(logFilePath, string.Empty);
        }
        catch (Exception ex)
        {
            Debug.Print($"Failed to clear log: {ex.Message}");
        }
    }

}
