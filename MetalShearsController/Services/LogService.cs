using System;
using System.IO;
using System.Diagnostics;

namespace MetalShearsController.Services;
// every time log is called its going to append the new log in file
public class LogService
{
    private static string logFilePath = "log.txt";

    public static void Log(string message)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
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
