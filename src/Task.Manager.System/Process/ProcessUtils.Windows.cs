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
    private static readonly Dictionary<string, string> userMap = new();

    internal static uint GetHandleCountInternal(SysDiag::Process process)
    {
        IntPtr processHandle = process.Handle;
        uint gdiHandleCount = WinUser.GetGuiResources(processHandle, WinUser.GR_GDIOBJECTS);
        uint usrHandleCount = WinUser.GetGuiResources(processHandle, WinUser.GR_USEROBJECTS);
        
        return (uint)process.HandleCount + (gdiHandleCount + usrHandleCount);
    }

    internal static string GetProcessCommandLine(in SysDiag::ProcessModule processModule, in string defaultValue)
    {
        // TODO: Kernel PEB + Commandline offset. 
        try {
            return processModule.FileName ?? defaultValue;
        }
        catch {
            return defaultValue;
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
   
    internal static string GetProcessProductName(
        in SysDiag::ProcessModule processModule,
        in int pid,
        string defaultValue)
    {
        try {
            string processPath = processModule.FileName;
            
            if (string.IsNullOrWhiteSpace(processPath)) {
                return defaultValue;
            }

            if (processPath.EndsWith(ServiceHost, StringComparison.CurrentCultureIgnoreCase)) {
                return GetProcessWin32ServiceName(pid, defaultValue);
            }

            SysDiag::FileVersionInfo versionInfo = SysDiag::FileVersionInfo.GetVersionInfo(processPath);
            
            return string.IsNullOrWhiteSpace(versionInfo.FileDescription) 
                ? defaultValue 
                : versionInfo.FileDescription;
        }
        catch {
            return defaultValue;            
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
    
    
    internal static string GetProcessUserName(in SysDiag::Process process)
    {
        try {
            if (ProcessThreadsApi.OpenProcessToken(
                process.SafeHandle,
                0x8u,
                out var tokenHandle)) {

                if (GetProcessTokenSid(tokenHandle, out SecurityIdentifier sid)) {
                    IdentityReference identityRef = sid.Translate(typeof(NTAccount));
                    var userName = identityRef.ToString();

                    if (userMap.ContainsKey(userName)) {
                        return userMap[userName];
                    }
                    
                    int domainIndex = userName.IndexOf('\\');
                    if (domainIndex != -1) {
                        var abbrevUserName = userName.Substring(domainIndex + 1);
                        userMap.Add(userName, abbrevUserName);
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

    private static string GetProcessWin32ServiceName(in int pid, string defaultValue)
    {
        /* TODO: Below code times out. Also, the thread continues to run on the
           timeout. Overlapped IO/IO Completion Ports would be good here.
         */
        
        /*
        string? serviceName = process.ProcessName;

        try {
            var task = global::System.Threading.Tasks.Task.Run(() => {
                var searcher = new ManagementObjectSearcher(
                    string.Format(MO_WIN32_SERVICE, pid));

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
            : defaultValue;
        */
        return defaultValue;
    }
#endif
}

#pragma warning restore CA1416 // Validate platform compatibility
