namespace Task.Manager.System.Process;

public class ProcessInfo
{
    public int Pid { get; set; }
    public int ThreadCount { get; set; }
    public uint HandleCount { get; set; }
    public long BasePriority { get; set; }
    public int ParentPid { get; set; }
    public DateTime StartTime { get; set; }

    public string ExeName { get; set; } = string.Empty;
    public string FileDescription { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string CmdLine { get; set; } = string.Empty;
    
    public long DiskUsage { get; set; }
    public ulong DiskOperations { get; set; }

    public long UsedMemory { get; set; }

    public double CpuTimePercent { get; set; }
    public double CpuUserTimePercent { get; set; }
    public double CpuKernelTimePercent { get; set; }
    
    public long PrevCpuKernelTime { get; set; }
    public long PrevCpuUserTime { get; set; }
    
    public long CurrCpuKernelTime { get; set; }
    public long CurrCpuUserTime { get; set; }
    
    public ProcessInfo() { }

    public ProcessInfo(ProcessInfo other)
    {
        // Copy constructor for deep-copy.
        Pid = other.Pid;
        ThreadCount = other.ThreadCount;
        HandleCount = other.HandleCount;
        BasePriority = other.BasePriority;
        ParentPid = other.ParentPid;
        StartTime = other.StartTime;
        ExeName = other.ExeName;
        FileDescription = other.FileDescription;
        UserName = other.UserName;
        CmdLine = other.CmdLine;
        DiskUsage = other.DiskUsage;
        DiskOperations = other.DiskOperations;
        UsedMemory = other.UsedMemory;
        CpuTimePercent = other.CpuTimePercent;
        CpuUserTimePercent = other.CpuUserTimePercent;
        CpuKernelTimePercent = other.CpuKernelTimePercent;
        PrevCpuKernelTime = other.PrevCpuKernelTime;
        PrevCpuUserTime = other.PrevCpuUserTime;
        CurrCpuKernelTime = other.CurrCpuKernelTime;
        CurrCpuUserTime = other.CurrCpuUserTime;
    }

    private bool Equals(ProcessInfo other)
    {
        return 
            Pid == other.Pid && 
            ThreadCount == other.ThreadCount && 
            HandleCount == other.HandleCount && 
            BasePriority == other.BasePriority && 
            ParentPid == other.ParentPid && 
            StartTime.Equals(other.StartTime) && 
            ExeName == other.ExeName && 
            FileDescription == other.FileDescription && 
            UserName == other.UserName && 
            CmdLine == other.CmdLine && 
            DiskUsage == other.DiskUsage && 
            DiskOperations == other.DiskOperations && 
            UsedMemory == other.UsedMemory && 
            CpuTimePercent.Equals(other.CpuTimePercent) && 
            CpuUserTimePercent.Equals(other.CpuUserTimePercent) && 
            CpuKernelTimePercent.Equals(other.CpuKernelTimePercent) && 
            PrevCpuKernelTime == other.PrevCpuKernelTime && 
            PrevCpuUserTime == other.PrevCpuUserTime && 
            CurrCpuKernelTime == other.CurrCpuKernelTime && 
            CurrCpuUserTime == other.CurrCpuUserTime;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) {
            return false;
        }

        if (ReferenceEquals(this, obj)) {
            return true;
        }

        if (obj.GetType() != GetType()) {
            return false;
        }
        
        return Equals((ProcessInfo)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Pid);
        hashCode.Add(ThreadCount);
        hashCode.Add(HandleCount);
        hashCode.Add(BasePriority);
        hashCode.Add(ParentPid);
        hashCode.Add(StartTime);
        hashCode.Add(ExeName);
        hashCode.Add(FileDescription);
        hashCode.Add(UserName);
        hashCode.Add(CmdLine);
        hashCode.Add(DiskUsage);
        hashCode.Add(DiskOperations);
        hashCode.Add(UsedMemory);
        hashCode.Add(CpuTimePercent);
        hashCode.Add(CpuUserTimePercent);
        hashCode.Add(CpuKernelTimePercent);
        hashCode.Add(PrevCpuKernelTime);
        hashCode.Add(PrevCpuUserTime);
        hashCode.Add(CurrCpuKernelTime);
        hashCode.Add(CurrCpuUserTime);
        return hashCode.ToHashCode();
    }
}


