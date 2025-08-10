using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Task.Manager.Cli.Utils;

public static class ExceptionHelper
{
    // Log diagnostic information for Release builds and use Asserts for Debug builds.
    [Conditional("DEBUG")]
    [Conditional("TRACE")]
    public static void HandleException(Exception ex) => HandleException(ex, string.Empty);
    
    [Conditional("DEBUG")]
    [Conditional("TRACE")]
    public static void HandleException(Exception ex, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) {
            message = ex.Message;
        }
#if TRACE
        Trace.TraceError($"An exception occurred. Message: {message}");
        Trace.TraceError($"Exception: {ex}");
#endif
#if DEBUG
        Debug.Assert(false, $"[DEBUG ASSERT]: Unexpected exception: {message}\n\n{ex}");
#endif
    }
}
