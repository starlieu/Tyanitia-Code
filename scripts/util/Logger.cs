using System;
using Godot;

public class Logger
{
    public static void Log(string message, bool error = false)
    {
        message = $"[{Time.GetDatetimeStringFromSystem()}] {message}";

        if (error)
        {
            GD.PrintErr(message);
        }
        else
        {
            GD.PrintRich(message);
        }
    }

    public static Exception Error(string message)
    {
        Log(message, true);

        return new Exception(message);
    }

    public static void Error(Exception exception)
    {
        Log($"{exception.Message}\n{exception.StackTrace.Replace("\n", "")}", true);
    }
}
