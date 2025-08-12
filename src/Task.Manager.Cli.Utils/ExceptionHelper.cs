using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Task.Manager.Cli.Utils;

public static class ExceptionHelper
{
    // Log diagnostic information for Release builds and use Asserts for Debug builds.
    public static void HandleException(Exception ex) => HandleException(ex, string.Empty);
    
    public static void HandleException(Exception ex, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) {
            message = ex.Message;
        }
        Trace.TraceError($"An exception occurred. Message: {message}");
        Trace.TraceError($"Exception: {ex}");
        Debug.Assert(false, $"[DEBUG ASSERT]: Unexpected exception: {message}\n\n{ex}");
    }
}
