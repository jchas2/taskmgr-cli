using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

using size_t = System.IntPtr;

public static unsafe class Sys
{
    public enum Selectors
    {
        CTL_HW = 0x06,              /* Hardware */
    }

    public enum Hardware
    {
        HW_MODEL = 0x02,            /* Cpu model */
        HW_PAGESIZE = 0x07,         /* Mem Page Size */
        HW_CPU_FREQ = 0x08,         /* Cpu frequency in Hz, does not work on Apple Silicon Mn chips */
        HW_MEMSIZE = 0x18,          /* Memory size in bytes */
    }
    
    private enum Error
    {
        ENOMEM = 0x10031,           /* Not enough space */
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

    private static unsafe bool Sysctl(
        int* name, 
        int name_len, 
        ref byte* value, 
        ref int len)
    {
        nint bytesLength = len;
        int result = -1;
        bool autoSize = (value == null && len == 0);

        if (autoSize) {
            result = Sysctl(
                name, 
                name_len, 
                value, 
                &bytesLength);

#if DEBUG
            Debug.Assert(result == 0, $"Failed Sysctl with error {result}");
#endif            
            if (result != 0) {
                return false;
            }

            value = (byte*)Marshal.AllocHGlobal((int)bytesLength);
        }

        result = Sysctl(
            name, 
            name_len, 
            value, 
            &bytesLength);
        
        while (autoSize && result != 0) { //&& GetLastErrorInfo().Error == Error.ENOMEM) {
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
            
            result = Sysctl(
                name, 
                name_len, 
                value, 
                &bytesLength);
        }

        if (result != 0) {
            if (autoSize) {
                Marshal.FreeHGlobal((IntPtr)value);
            }

            //throw new InvalidOperationException(SR.Format(SR.InvalidSysctl, *name, Marshal.GetLastPInvokeError()));
            return false;
        }

        len = (int)bytesLength;

        return true;
    }
}
