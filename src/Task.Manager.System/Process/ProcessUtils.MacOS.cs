using SysDiag = System.Diagnostics;

namespace Task.Manager.System.Process;

public static partial class ProcessUtils
{
#if __APPLE__
    public static uint GetHandleCountInternal(SysDiag::Process process)
    {
        return process.HandleCount;
    }
#endif
}
