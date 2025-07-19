using System.Runtime.InteropServices;

namespace Task.Manager.Interop.Win32;

public static class WinNt
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public unsafe struct SID_AND_ATTRIBUTES
    {
        public uint* Sid;
        public uint Attributes;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct TOKEN_USER
    {
        public SID_AND_ATTRIBUTES sidAndAttributes;
    }
}
