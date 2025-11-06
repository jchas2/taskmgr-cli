using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ServiceProcess;
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

    private static int GetPriorityInternal(in SysDiag::Process process) => process.BasePriority;

    internal static string GetProcessCommandLine(in int pid, in SysDiag::ProcessModule processModule, in string defaultValue)
    {
        try {
            if (ServiceUtils.GetService(pid, out ServiceController? serviceController)) {
                string imagePath = ServiceUtils.GetServiceImagePath(serviceController!.ServiceName) ?? defaultValue;
                return Environment.ExpandEnvironmentVariables(imagePath);
            }

            // TODO: Kernel PEB + Commandline offset. 
            return processModule.FileName;
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

            if (ServiceUtils.GetService(pid, out ServiceController? serviceController)) {
                return serviceController?.DisplayName ?? defaultValue;
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
            SecurityIdentifier? sid = GetProcessSecurityIdentifier(process);

            if (sid == null) {
                return DefaultUser;
            }
            
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
        catch (Exception e) {
            SysDiag.Trace.WriteLine(e);
        }
        
        return DefaultUser;
    }

    internal static UserRoleFlags GetProcessUserRoles(in SysDiag::Process process)
    {
        UserRoleFlags flags = 0;
        SecurityIdentifier? sid = GetProcessSecurityIdentifier(process);

        if (sid == null) {
            return flags;
        }

        if (sid.IsWellKnown(WellKnownSidType.LocalSystemSid) || sid.Value == "S-1-5-18" ||
            sid.IsWellKnown(WellKnownSidType.LocalServiceSid) || sid.Value == "S-1-5-19" ||
            sid.IsWellKnown(WellKnownSidType.NetworkServiceSid) || sid.Value == "S-1-5-20") {

            flags |= UserRoleFlags.SystemUser;
        }
                
        // TODO:
        // Process user is member of Administrators group, or is running with User Elevation (UAC)
        // flags |= UserRoleFlags.RootUser;
        
        // Process user is different to the user running taskmgr. 
        // flags |= UserRoleFlags.OtherUser;
        
        return flags;
    }

    private static SecurityIdentifier? GetProcessSecurityIdentifier(in SysDiag::Process process)
    {
        if (ProcessThreadsApi.OpenProcessToken(
            process.SafeHandle,
            0x8u,
            out var tokenHandle)) {

            if (GetProcessTokenSid(tokenHandle, out SecurityIdentifier sid)) {
                return sid;
            }
        }

        return null;
    }

    internal static bool IsDaemonInternal(in int pid) => 
        ServiceUtils.GetService(Pid, out ServiceController? _);
    
    internal static unsafe bool IsLowPriorityInternal(in SysDiag::Process process) =>
        GetPriorityInternal(process) < 8;
#endif
}

#pragma warning restore CA1416 // Validate platform compatibility
