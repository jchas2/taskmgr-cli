using System.Diagnostics;
using System.Runtime.InteropServices;
using Task.Manager.Interop.Mach;
using Task.Manager.System.Process;

namespace Task.Manager;

public class Processor
{
    private readonly IProcesses _processes;

    public Processor(IProcesses processes)
    {
        _processes = processes ?? throw new ArgumentNullException(nameof(processes));
    }
    
    public IList<ProcessInfo> GetProcesses()
    {
        var allProcs = _processes.GetAll();



        var cpuLoadInfo = GetHostCpuLoadInfo();

        return allProcs;
    }

    private MachHost.HostCpuLoadInfo GetHostCpuLoadInfo()
    {
        IntPtr host = MachHost.host_self();
        int count = (int)(Marshal.SizeOf(typeof(MachHost.HostCpuLoadInfo)) / sizeof(int));
        
        var info = new MachHost.HostCpuLoadInfo() {
            idle = 0,
            nice = 0,
            system = 0,
            user = 0
        };
        
        IntPtr infoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(info));
        
        if (MachHost.host_statistics(
                host,
                MachHost.HOST_CPU_LOAD_INFO,
                infoPtr,
                ref count) == 0) {

            info = Marshal.PtrToStructure<MachHost.HostCpuLoadInfo>(infoPtr);
            return info;
        }
        else {
            int error = Marshal.GetLastWin32Error();
            Debug.Write(error);
        }

        return info;
    }    
}
