using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

using size_t = System.IntPtr;

public static unsafe class Sys
{
    private enum Error
    {
        ENOMEM = 0x10031,           // Not enough space.
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
        int ret = -1;
        bool autoSize = (value == null && len == 0);

        if (autoSize) {
            ret = Sysctl(
                name, 
                name_len, 
                value, 
                &bytesLength);

#if DEBUG
            Debug.Assert(ret == 0, $"Failed Sysctl with error {ret}");
#endif            
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
        
        while (autoSize && ret != 0) { //&& GetLastErrorInfo().Error == Error.ENOMEM) {
            // Do not use ReAllocHGlobal() here: we don't care about
            // previous contents, and proper checking of value returned
            // will make code more complex.
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
            
            ret = Sysctl(
                name, 
                name_len, 
                value, 
                &bytesLength);
        }

        if (ret != 0) {
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
