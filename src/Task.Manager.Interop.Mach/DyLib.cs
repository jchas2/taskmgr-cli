using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

// Following declarations are found in the Mach Kernel header file dylib.h

public sealed class DyLib
{
    [DllImport(Libraries.LibSystemDyLib)]
    public static extern uint _dyld_image_count();

    [DllImport(Libraries.LibSystemDyLib)]
    public static extern IntPtr _dyld_get_image_name(uint image_index);
}