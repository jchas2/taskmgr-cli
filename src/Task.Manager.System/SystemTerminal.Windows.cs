using System.Diagnostics;
using System.Runtime.InteropServices;
using Task.Manager.Interop.Win32;

namespace Task.Manager.System;

public partial class SystemTerminal
{
#if __WIN32__    
    private bool CursorVisibleInternal
    {
        get {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return Console.CursorVisible;
            }

            return false;
        }
        set {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Console.CursorVisible = value;
            }
        }
    }

    private void EnableAnsiTerminalCodesInternal()
    {
        // Not all terminals on Windows platforms (cmd.exe as an example) support ANSI escape codes.
        // We attempt to enable here to support older terminals.
        
        IntPtr consoleHandle = ProcessEnv.GetStdHandle(ProcessEnv.STD_OUTPUT_HANDLE);
        if (consoleHandle == IntPtr.Zero || consoleHandle == new IntPtr(-1)) {
#if DEBUG
            int error = Marshal.GetLastWin32Error();
            Debug.Assert(error == 0, $"Failed GetStdHandle(): {Marshal.GetPInvokeErrorMessage(error)}");
#endif
            return;
        }

        if (!ConsoleApi.GetConsoleMode(consoleHandle, out uint originalMode)) {
#if DEBUG
            int error = Marshal.GetLastWin32Error();
            Debug.Assert(error == 0, $"Failed GetConsoleMode(): {Marshal.GetPInvokeErrorMessage(error)}");
#endif
            return;
        }

        uint newMode = originalMode | ConsoleApi.ENABLE_VIRTUAL_TERMINAL_PROCESSING;
        if (!ConsoleApi.SetConsoleMode(consoleHandle, newMode)) {
#if DEBUG
            int error = Marshal.GetLastWin32Error();
            Debug.Assert(error == 0, $"Failed SetConsoleMode(): {Marshal.GetPInvokeErrorMessage(error)}");
#endif
        }
    }
#endif
}