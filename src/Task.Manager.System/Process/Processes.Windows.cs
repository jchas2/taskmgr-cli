using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using Task.Manager.Interop.Win32;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

#pragma warning disable CA1416 // Validate platform compatibility        

public partial class Processes : IProcesses
{
#if __WIN32__    
    private const string DEFAULT_USER = "SYSTEM";
    private const string SERVICE_HOST = "svchost.exe";
    private static Dictionary<string, string> _userMap = new();

    private string GetProcessCommandLine(global::System.Diagnostics.Process process)
    {
        // TODO: Kernel PEB + Commandline offset. 
        try {
            return process.MainModule?.FileName ?? process.ProcessName;
        }
        catch {
            return process.ProcessName;
        } 
    }
    
    private string GetProcessProductName(SysDiag::Process process)
    {
        try {
            string? processPath = process.MainModule?.FileName;
            
            if (string.IsNullOrWhiteSpace(processPath)) {
                return process.ProcessName;
            }

            if (processPath.EndsWith(SERVICE_HOST, StringComparison.CurrentCultureIgnoreCase)) {
                return GetProcessWin32ServiceName(process);
            }
            
            var versionInfo = FileVersionInfo.GetVersionInfo(processPath);
            return versionInfo.FileDescription ?? process.ProcessName;
        }
        catch {
            return process.ProcessName;            
        }
    }
    
    private unsafe bool GetProcessTokenSid(SafeProcessHandle processHandle, out SecurityIdentifier sid)
    {
        bool result = false;
        const int BUF_LENGTH = 256;
        const int TOKEN_USER = 1;
        
        sid = new SecurityIdentifier(WellKnownSidType.NullSid, null);

        try {
            byte[] buffer = new byte[BUF_LENGTH];
            fixed (byte* tokenInfo = &buffer[0]) {
                uint bufLength = BUF_LENGTH;

                result = SecurityBaseApi.GetTokenInformation(
                    processHandle,
                    TOKEN_USER,
                    (uint*)tokenInfo,
                    BUF_LENGTH,
                    &bufLength);

                if (result) {
                    WinNt.TOKEN_USER* tokenUser = (WinNt.TOKEN_USER*)tokenInfo;
                    
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                        uint* sidPtr = tokenUser->sidAndAttributes.Sid;
                        sid = new SecurityIdentifier(new IntPtr(sidPtr));
                    }
                }

                return result;
            }
        }
        catch (Exception ex) {
#if DEBUG
            Debug.WriteLine(ex.Message);
#endif
            return result;
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

    private string GetProcessWin32ServiceName(SysDiag::Process process)
    {
        /* TODO: Below code times out. Also, the thread continues to run on the
           timeout. Overlapped IO/IO Completion Ports would be good here.
         */
        
        /*
        string? serviceName = process.ProcessName;

        try {
            var task = global::System.Threading.Tasks.Task.Run(() => {
                var searcher = new ManagementObjectSearcher(
                    string.Format(MO_WIN32_SERVICE, process.Id));

                foreach (var obj in searcher.Get()) {
                    var name = obj["Name"]?.ToString();
                    if (false == string.IsNullOrWhiteSpace(serviceName)) {
                        serviceName = name;
                        break;
                    }
                    else {
                        break;
                    }
                }
            });

            task.Wait(20);
        }
        catch {
            
        }
        
        return false == string.IsNullOrWhiteSpace(serviceName) 
            ? $"Service Host: {serviceName}" 
            : process.ProcessName;
        */
        return process.ProcessName;
    }
#endif
}

#pragma warning restore CA1416 // Validate platform compatibility
