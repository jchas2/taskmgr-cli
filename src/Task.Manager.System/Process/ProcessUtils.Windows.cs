using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using Task.Manager.Interop.Win32;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

#pragma warning disable CA1416 // Validate platform compatibility        

public static partial class ProcessUtils
{
#if __WIN32__
    private const string DefaultUser = "SYSTEM";
    private const string ServiceHost = "svchost.exe";
    private static Dictionary<string, string> _userMap = new();

    public static uint GetHandleCountInternal(SysDiag::Process process)
    {
        IntPtr processHandle = process.Handle;
        uint gdiHandleCount = WinUser.GetGuiResources(processHandle, WinUser.GR_GDIOBJECTS);
        uint usrHandleCount = WinUser.GetGuiResources(processHandle, WinUser.GR_USEROBJECTS);
        
        return (uint)process.HandleCount + (gdiHandleCount + usrHandleCount);
    }

    public static string GetProcessCommandLine(global::System.Diagnostics.Process process)
    {
        // TODO: Kernel PEB + Commandline offset. 
        try {
            return process.MainModule?.FileName ?? process.ProcessName;
        }
        catch {
            return process.ProcessName;
        } 
    }

    public static ulong GetProcessIoOperations(in int pid)
    {
        if (!TryGetProcessByPid(pid, out SysDiag::Process? process)) {
            return 0;
        }
        
        return GetProcessIoOperations(process!);
    }

    public static ulong GetProcessIoOperations(in SysDiag::Process process)
    {
        try {
            WinNt.IO_COUNTERS counters = new();

            if (!WinNt.GetProcessIoCounters(process.Handle, out counters)) {
                Win32ErrorHelpers.AssertOnLastError(nameof(WinNt.GetProcessIoCounters));
                return 0;
            }

            return counters.ReadTransferCount + counters.WriteTransferCount;
        }
        catch {
            return 0;
        }
    }
   
    public static string GetProcessProductName(SysDiag::Process process)
    {
        try {
            string? processPath = process.MainModule?.FileName;
            
            if (string.IsNullOrWhiteSpace(processPath)) {
                return process.ProcessName;
            }

            if (processPath.EndsWith(ServiceHost, StringComparison.CurrentCultureIgnoreCase)) {
                return GetProcessWin32ServiceName(process);
            }

            SysDiag.FileVersionInfo versionInfo = SysDiag.FileVersionInfo.GetVersionInfo(processPath);
            
            return string.IsNullOrWhiteSpace(versionInfo.FileDescription) 
                ? process.ProcessName 
                : versionInfo.FileDescription;
        }
        catch {
            return process.ProcessName;            
        }
    }
    
    private static unsafe bool GetProcessTokenSid(SafeProcessHandle processHandle, out SecurityIdentifier sid)
    {
        var result = false;
        const int BufferLength = 256;
        const int TokenUser = 1;
        
        sid = new SecurityIdentifier(WellKnownSidType.NullSid, null);

        try {
            byte[] buffer = new byte[BufferLength];
            fixed (byte* tokenInfo = &buffer[0]) {
                uint bufLength = BufferLength;

                result = SecurityBaseApi.GetTokenInformation(
                    processHandle,
                    TokenUser,
                    (uint*)tokenInfo,
                    BufferLength,
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
            SysDiag.Trace.WriteLine(ex.Message);
            return result;
        }
    }
    
    
    public static string GetProcessUserName(SysDiag::Process process)
    {
        try {
            if (ProcessThreadsApi.OpenProcessToken(
                process.SafeHandle,
                0x8u,
                out var tokenHandle)) {

                if (GetProcessTokenSid(tokenHandle, out SecurityIdentifier sid)) {
                    IdentityReference identityRef = sid.Translate(typeof(NTAccount));
                    var userName = identityRef.ToString();

                    if (_userMap.ContainsKey(userName)) {
                        return _userMap[userName];
                    }
                    
                    int domainIndex = userName.IndexOf('\\');
                    if (domainIndex != -1) {
                        var abbrevUserName = userName.Substring(domainIndex + 1);
                        _userMap.Add(userName, abbrevUserName);
                    }
                    
                    return userName;
                }
            }
        }
        catch (Exception e) {
            SysDiag.Trace.WriteLine(e);
        }
        
        return DefaultUser;
    }

    private static string GetProcessWin32ServiceName(SysDiag::Process process)
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
