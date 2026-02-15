using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

public static class IOKit
{
    [DllImport(Libraries.IOKit)]
    public static extern uint IOIteratorNext(IntPtr iterator);

    [DllImport(Libraries.IOKit)]
    public static extern int IOObjectRelease(uint obj);

    [DllImport(Libraries.IOKit)]
    public static extern int IOObjectRelease(IntPtr obj);
    
    [DllImport(Libraries.IOKit)]
    public static extern int IORegistryEntryCreateCFProperties(
        uint entry, 
        out IntPtr properties, 
        IntPtr allocator, 
        uint options);
    
    [DllImport(Libraries.IOKit)]
    public static extern int IORegistryEntryGetChildIterator(
        uint entry, 
        string plane, 
        ref IntPtr iterator);
    
    [DllImport(Libraries.IOKit)]
    public static extern IntPtr IOServiceMatching(string name);

    [DllImport(Libraries.IOKit)]
    public static extern uint IOServiceGetMatchingService(uint masterPort, IntPtr matching);
}
