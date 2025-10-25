using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

public sealed class UniStd
{
    [DllImport(Libraries.LibC)]
    public static extern uint geteuid();
}
