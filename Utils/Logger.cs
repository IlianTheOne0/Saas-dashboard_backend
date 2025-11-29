namespace Utils;

using System;

public enum LogLevel
{
    DEBUG,
    INFO,
    WARN,
    ERROR,
    FATAL
}

public static class Logger
{
    private static readonly object _lock = new object();

    public static void Log(LogLevel level, string source, string message, Exception? error = null)
    {
        lock (_lock)
        {
            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = level switch
            {
                LogLevel.DEBUG => ConsoleColor.Gray,
                LogLevel.INFO => ConsoleColor.Green,
                LogLevel.WARN => ConsoleColor.Yellow,
                LogLevel.ERROR => ConsoleColor.Red,
                LogLevel.FATAL => ConsoleColor.Magenta,
                _ => ConsoleColor.White,
            };

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] [{level.ToString().PadRight(5)}] [{source}] {message}";

            Console.WriteLine(logEntry);

            if (error != null)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"\t[EXCEPTION] {error.GetType().Name}: {error.Message}");
                if (error.StackTrace != null) { Console.WriteLine($"\t{error.StackTrace.Replace("\n", "\n\t")}"); }
            }

            Console.ForegroundColor = originalColor;
        }
    }
    public static void Debug(string source, string message) => Log(LogLevel.DEBUG, source, message);
    public static void Info(string source, string message) => Log(LogLevel.INFO, source, message);
    public static void Warn(string source, string message) => Log(LogLevel.WARN, source, message);
    public static void Error(string source, string message, Exception? error = null) => Log(LogLevel.ERROR, source, message, error);
    public static void Fatal(string source, string message, Exception? error = null) => Log(LogLevel.FATAL, source, message, error);
}