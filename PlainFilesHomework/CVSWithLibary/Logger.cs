using System.Globalization;

namespace CVSWithLibary;

public static class Logger
{
    private const string LogFilePath = "log.txt";
    private static string? _currentUser = "UNAUTHENTICATED";

    public static void SetCurrentUser(string username)
    {
        _currentUser = username;
    }

    public static void Log(string message)
    {
        try
        {
            string logEntry = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}][{_currentUser}] {message}";
            File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to log: {ex.Message}");
        }
    }
}
