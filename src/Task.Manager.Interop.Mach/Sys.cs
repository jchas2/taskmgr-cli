using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

using size_t = System.IntPtr;

public static unsafe class Sys
{
    public enum Selectors
    {
        CTL_KERN = 0x01,            /* Kernel */
        CTL_VM = 0x02,              /* Virtual Memory */
        CTL_HW = 0x06,              /* Hardware */
    }
    
    public enum Hardware
    {
        HW_MODEL = 0x02,            /* Cpu model */
        HW_PAGESIZE = 0x07,         /* Mem Page Size */
        HW_CPU_FREQ = 0x08,         /* Cpu frequency in Hz, does not work on Apple Silicon Mn chips */
        HW_MEMSIZE = 0x18,          /* Memory size in bytes */
    }

    public const int VM_SWAPUSAGE = 5;
    
    private enum Error
    {
        ENOMEM = 0x10031,           /* Not enough space */
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XswUsage
    {
        public ulong total;
        public ulong avail;
        public ulong used;
        public uint  pagesize;
        public uint  encrypted;
    }
    
    [DllImport(Libraries.LibSystemNative, EntryPoint = "SystemNative_Sysctl", SetLastError = true)]
    private static extern unsafe int Sysctl(
        int* name,
        int namelen,
        void* value,
        size_t* len);

    public static unsafe bool Sysctl(ReadOnlySpan<int> name, ref byte* value, ref int len)
    {
        fixed (int* ptr = &MemoryMarshal.GetReference(name)) {
            return Sysctl(ptr, name.Length, ref value, ref len);
        }
    }
    
    private static unsafe bool Sysctl(int* name, int name_len, ref byte* value, ref int len)
    {
        nint bytesLength = len;
        int ret = -1;
        bool autoSize = (value == null && len == 0);

        if (autoSize) {
            ret = Sysctl(name, name_len, value, &bytesLength);
            
            if (ret != 0) {
                return false;
            }
            
            value = (byte*)Marshal.AllocHGlobal((int)bytesLength);
        }

        ret = Sysctl(
            name, 
            name_len, 
            value, 
            &bytesLength);
        
        int lastError = Marshal.GetLastPInvokeError();
        
        while (autoSize && ret != 0 && lastError == (int)Error.ENOMEM)
        {
            Marshal.FreeHGlobal((IntPtr)value);
            
            if ((int)bytesLength == int.MaxValue) {
                throw new OutOfMemoryException();
            }
            
            if ((int)bytesLength >= int.MaxValue / 2) {
                bytesLength = int.MaxValue;
            } 
            else {
                bytesLength = (int)bytesLength * 2;
            }
            
            value = (byte*)Marshal.AllocHGlobal(bytesLength);
            ret = Sysctl(name, name_len, value, &bytesLength);
        }
        
        if (ret != 0) {
            if (autoSize) {
                Marshal.FreeHGlobal((IntPtr)value);
            }
            
            return false;
        }

        len = (int)bytesLength;

        return true;
    }
}
