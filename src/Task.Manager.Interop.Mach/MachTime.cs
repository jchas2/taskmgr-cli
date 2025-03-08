using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

// Following declarations are found in the Mach Kernel header file mach_time.h

public class MachTime
{
    public struct mach_timebase_info_data_t
    {
        public uint numer;
        public uint denom;
    }
    
    [DllImport(Libraries.LibSystemDyLib)]
    public static extern unsafe int mach_timebase_info(mach_timebase_info_data_t* info);
}