using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

public sealed class DyLib
{
    [DllImport(Libraries.LibSystemDyLib)]
    public static extern uint _dyld_image_count();

    [DllImport(Libraries.LibSystemDyLib)]
    public static extern IntPtr _dyld_get_image_name(uint image_index);
}