using System.Runtime.Versioning;
using System.ServiceProcess;
using Task.Manager.System.Process;
using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Tests.Process;

public class ServiceUtilsTests
{
    [SkippableFact]
    [SupportedOSPlatform("windows")]
    public void Should_Get_Services_With_Pid()
    {
        ProcessService processService = new();
        
        bool foundAnyService = processService.GetProcesses()
            .Select(p => ServiceUtils.GetService(p.Pid, out _))
            .Any();

        Assert.True(foundAnyService);
    }
    
    [SkippableFact]
    [SupportedOSPlatform("windows")]
    public void Should_Get_Services_With_ImagePath()
    {
        ProcessService processService = new();

        bool foundAnyService = processService.GetProcesses()
            .Select(p => {
                if (ServiceUtils.GetService(p.Pid, out ServiceController? sc)) {
                    return sc;
                }
                return null;
            })
            .Where(sc => sc != null)
            .Any(sc => !string.IsNullOrEmpty(ServiceUtils.GetServiceImagePath(sc!.ServiceName)));

        Assert.True(foundAnyService);
    }
}
