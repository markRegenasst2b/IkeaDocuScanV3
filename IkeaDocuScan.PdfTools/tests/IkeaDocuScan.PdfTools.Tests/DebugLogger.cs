using System.Text;

namespace IkeaDocuScan.PdfTools.Tests;

/// <summary>
/// Simple file logger for debugging test issues.
/// </summary>
public static class DebugLogger
{
    private static readonly string LogFilePath = Path.Combine(
        Directory.GetCurrentDirectory(),
        $"test-debug-{DateTime.Now:yyyyMMdd-HHmmss}.log"
    );

    private static readonly object _lock = new object();

    static DebugLogger()
    {
        // Clear any existing log file
        try
        {
            if (File.Exists(LogFilePath))
            {
                File.Delete(LogFilePath);
            }
            Log($"=== Test Debug Log Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            Log($"Log file: {LogFilePath}");
        }
        catch
        {
            // Ignore errors during logger initialization
        }
    }

    public static void Log(string message)
    {
        lock (_lock)
        {
            try
            {
                var logMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
                Console.WriteLine(logMessage);
                File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }

    public static void LogBytes(string label, byte[] bytes, int maxBytes = 20)
    {
        if (bytes == null)
        {
            Log($"{label}: NULL");
            return;
        }

        var hexBytes = string.Join(" ", bytes.Take(maxBytes).Select(b => b.ToString("X2")));
        Log($"{label}: {bytes.Length} bytes, first {maxBytes}: {hexBytes}");
    }

    public static string GetLogFilePath() => LogFilePath;
}
