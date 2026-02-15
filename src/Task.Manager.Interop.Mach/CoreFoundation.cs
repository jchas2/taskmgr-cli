using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Mach;

public static class CoreFoundation
{
    [DllImport(Libraries.CoreFoundation)]
    public static extern long CFArrayGetCount(IntPtr array);

    [DllImport(Libraries.CoreFoundation)]
    public static extern IntPtr CFArrayGetValueAtIndex(IntPtr array, long index);

    [DllImport(Libraries.CoreFoundation)]
    public static extern void CFDictionaryGetKeysAndValues(
        IntPtr dict,
        IntPtr[] keys,
        IntPtr[] values);

    [DllImport(Libraries.CoreFoundation)]
    public static extern long CFDictionaryGetCount(IntPtr dict);
    
    [DllImport(Libraries.CoreFoundation)]
    public static extern long CFGetTypeID(IntPtr cf);

    [DllImport(Libraries.CoreFoundation)]
    public static extern bool CFNumberGetValue(IntPtr number, int theType, out long valuePtr);
    
    [DllImport(Libraries.CoreFoundation)]
    public static extern void CFRelease(IntPtr cf);

    [DllImport(Libraries.CoreFoundation)]
    public static extern bool CFStringGetCString(
        IntPtr theString,
        byte[] buffer,
        long bufferSize,
        int encoding);

    private const int kCFStringEncodingUTF8 = 0x08000100;
    private const int kCFNumberSInt64Type = 4;
    
    public static void CFNumberGetValue(IntPtr number, out long value)
    {
        value = 0;
        CFNumberGetValue(number, kCFNumberSInt64Type, out value);
    }

    public static string? GetString(IntPtr cfString)
    {
        if (cfString == IntPtr.Zero)
            return null;

        byte[] buffer = new byte[1024];
        
        if (CFStringGetCString(cfString, buffer, buffer.Length, kCFStringEncodingUTF8)) {
            int length = Array.IndexOf<byte>(buffer, 0);
            
            if (length >= 0) {
                return System.Text.Encoding.UTF8.GetString(buffer, 0, length);
            }
        }

        return null;
    }
    
    public static Dictionary<string, IntPtr> ToDictionary(IntPtr cfDict)
    {
        Dictionary<string, IntPtr> result = new();

        if (cfDict == IntPtr.Zero) {
            return result;
        }

        long count = CFDictionaryGetCount(cfDict);
        
        if (count == 0) {
            return result;
        }

        IntPtr[] keys = new IntPtr[count];
        IntPtr[] values = new IntPtr[count];

        CFDictionaryGetKeysAndValues(
            cfDict, 
            keys, 
            values);

        for (int i = 0; i < count; i++)
        {
            string? key = GetString(keys[i]);
            
            if (key != null) {
                result[key] = values[i];
            }
        }

        return result;
    }
}