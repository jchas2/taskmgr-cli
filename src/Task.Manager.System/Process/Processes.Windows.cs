using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using Task.Manager.Interop.Win32;

namespace Task.Manager.System.Process;

#pragma warning disable CA1416 // Validate platform compatibility        

public partial class Processes : IProcesses
{
#if __WIN32__    
    private const string DEFAULT_USER = "SYSTEM";
    private static Dictionary<string, string> _userMap = new();

    private bool GetProcessTokenSid(SafeProcessHandle processHandle, out SecurityIdentifier sid)
    {
        bool result = false;
        IntPtr tokenInfo = IntPtr.Zero;
        const int BUF_LENGTH = 256;

        sid = new SecurityIdentifier(WellKnownSidType.NullSid, null);

        try {
            tokenInfo = Marshal.AllocHGlobal(BUF_LENGTH);
            int bufLength = BUF_LENGTH;
            
            result = SecurityBaseApi.GetTokenInformation(
                processHandle, 
                1, 
                tokenInfo, 
                bufLength, 
                ref bufLength);

            if (result) {
                WinNt.TOKEN_USER tokenUser = Marshal.PtrToStructure<WinNt.TOKEN_USER>(tokenInfo);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    sid = new SecurityIdentifier(tokenUser.sidAndAttributes.Sid);
                }
            }

            return result;
        }
        catch (Exception ex) {
#if DEBUG
            Debug.WriteLine(ex.Message);
#endif
            return result;
        }
        finally {
            if (tokenInfo != IntPtr.Zero) {
                Marshal.FreeHGlobal(tokenInfo);
            }
        }
    }
    
    
    private string GetProcessUserName(global::System.Diagnostics.Process process)
    {
        try {
            if (ProcessThreadsApi.OpenProcessToken(
                process.SafeHandle,
                0x8u,
                out var tokenHandle)) {

                if (GetProcessTokenSid(tokenHandle, out SecurityIdentifier sid)) {
                    var identityRef = sid.Translate(typeof(NTAccount));
                    string userName = identityRef.ToString();

                    if (_userMap.ContainsKey(userName)) {
                        return _userMap[userName];
                    }
                    
                    int domainIndex = userName.IndexOf('\\');
                    if (domainIndex != -1) {
                        string abbrevUserName = userName.Substring(domainIndex + 1);
                        _userMap.Add(userName, abbrevUserName);
                    }
                    
                    return userName;
                }
            }
        }
        catch (Exception e) {
#if DEBUG
            /* */
            Debug.WriteLine(e);
#endif
        }
        
        return DEFAULT_USER;
    }
#endif
}

#pragma warning restore CA1416 // Validate platform compatibility
