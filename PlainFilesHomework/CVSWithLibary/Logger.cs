using System.Globalization;

namespace CVSWithLibary;

public static class Logger
{
    private const string LogFilePath = "log.txt";
    private static string? _currentUser = "UNAUTHENTICATED"; // Usuario por defecto para el log antes de iniciar 

    public static void SetCurrentUser(string username)
    {
        _currentUser = username;
    }

    public static void Log(string message)
    {
        try
        {
            // Formato: [AAAA-MM-DD HH:MM:SS][USUARIO] MENSAJE
            string logEntry = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}][{_currentUser}] {message}";
            File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al escribir en el log: {ex.Message}");
            // Opcionalmente, podrías registrar este error en la Consola o en un log de errores separado
        }
    }
}
