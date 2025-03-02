using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Task.Manager.Interop.Win32;

// Following declarations are found in the platform sdk header file SecurityBaseApi.h

public class SecurityBaseApi
{
    [DllImport(Libraries.Advapi32, CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool GetTokenInformation(
        SafeProcessHandle tokenHandle, 
        uint tokenInformationClass, 
        IntPtr tokenInformation, 
        int tokenInformationLength, 
        ref int returnLength);
}