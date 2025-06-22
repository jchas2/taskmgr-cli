using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

// Following declarations are found in the Unix Standard header file unistd.h

public sealed class UniStd
{
    [DllImport(Libraries.LibC)]
    public static extern uint geteuid();
}
