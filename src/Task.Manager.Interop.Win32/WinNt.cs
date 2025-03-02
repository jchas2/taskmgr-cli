using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Win32;

// Following declarations are found in the platform sdk header file WinNt.h

public class WinNt
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct SID_AND_ATTRIBUTES
    {
        public IntPtr Sid;
        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct TOKEN_USER
    {
        public SID_AND_ATTRIBUTES sidAndAttributes;
    }
}