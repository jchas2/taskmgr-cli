using System.Reflection;
using Task.Manager.Process;

namespace Task.Manager.Tests.Process;

public sealed class ProcessorInfoTests
{
    [Fact]
    public void ProcessorInfo_Canary_Test()
    {
        // If this fails, review all tests in this class                                                                                   
        // and update the expected count after adding tests                                                                                
        const int ExpectedPropertyCount = 22;

        int actualCount = typeof(ProcessorInfo).GetProperties(
            BindingFlags.Public | BindingFlags.Instance).Length;

        Assert.Equal(ExpectedPropertyCount, actualCount);
    }

    [Fact]                                                                                                                             
    public void Default_Constructor_Initializes_With_Default_Values()                                                                  
    {                                                                                                                                  
        ProcessorInfo info = new();                                                                                                    
                                                                                                                                     
        Assert.Equal(0, info.Pid);                                                                                                     
        Assert.Equal(0, info.ThreadCount);                                                                                             
        Assert.Equal(0U, info.HandleCount);                                                                                            
        Assert.Equal(0, info.BasePriority);                                                                                            
        Assert.Equal(0, info.ParentPid);                                                                                               
        Assert.False(info.IsDaemon);                                                                                                   
        Assert.False(info.IsLowPriority);                                                                                              
        Assert.Equal(default(DateTime), info.StartTime);                                                                               
        Assert.Equal(string.Empty, info.ProcessName);                                                                                  
        Assert.Equal(string.Empty, info.FileDescription);                                                                              
        Assert.Equal(string.Empty, info.UserName);                                                                                     
        Assert.Equal(string.Empty, info.CmdLine);                                                                                      
        Assert.Equal(0, info.DiskUsage);                                                                                               
        Assert.Equal(0UL, info.DiskOperations);                                                                                        
        Assert.Equal(0, info.UsedMemory);                                                                                              
        Assert.Equal(0.0, info.CpuTimePercent);                                                                                        
        Assert.Equal(0.0, info.CpuUserTimePercent);                                                                                    
        Assert.Equal(0.0, info.CpuKernelTimePercent);                                                                                  
        Assert.Equal(0, info.PrevCpuKernelTime);                                                                                       
        Assert.Equal(0, info.PrevCpuUserTime);                                                                                         
        Assert.Equal(0, info.CurrCpuKernelTime);                                                                                       
        Assert.Equal(0, info.CurrCpuUserTime);                                                                                         
    }                                                                                                                                  
                                                                                                                                         
    [Fact]                                                                                                                             
    public void Properties_Can_Be_Set_And_Retrieved()                                                                                  
    {                                                                                                                                  
        DateTime startTime = DateTime.Now;                                                                                             
        ProcessorInfo info = new() {                                                                                                   
            Pid = 1234,                                                                                                                
            ThreadCount = 10,                                                                                                          
            HandleCount = 50,                                                                                                          
            BasePriority = 8,                                                                                                          
            ParentPid = 1,                                                                                                             
            IsDaemon = true,                                                                                                           
            IsLowPriority = false,                                                                                                     
            StartTime = startTime,                                                                                                     
            ProcessName = "test.exe",                                                                                                  
            FileDescription = "Test Process",                                                                                          
            UserName = "testuser",                                                                                                     
            CmdLine = "/usr/bin/test --arg",                                                                                           
            DiskUsage = 1024,                                                                                                          
            DiskOperations = 100,                                                                                                      
            UsedMemory = 2048,                                                                                                         
            CpuTimePercent = 15.5,                                                                                                     
            CpuUserTimePercent = 10.2,                                                                                                 
            CpuKernelTimePercent = 5.3,                                                                                                
            PrevCpuKernelTime = 1000,                                                                                                  
            PrevCpuUserTime = 2000,                                                                                                    
            CurrCpuKernelTime = 1100,                                                                                                  
            CurrCpuUserTime = 2200                                                                                                     
        };                                                                                                                             
                                                                                                                                     
        Assert.Equal(1234, info.Pid);                                                                                                  
        Assert.Equal(10, info.ThreadCount);                                                                                            
        Assert.Equal(50U, info.HandleCount);                                                                                           
        Assert.Equal(8, info.BasePriority);                                                                                            
        Assert.Equal(1, info.ParentPid);                                                                                               
        Assert.True(info.IsDaemon);                                                                                                    
        Assert.False(info.IsLowPriority);                                                                                              
        Assert.Equal(startTime, info.StartTime);                                                                                       
        Assert.Equal("test.exe", info.ProcessName);                                                                                    
        Assert.Equal("Test Process", info.FileDescription);                                                                            
        Assert.Equal("testuser", info.UserName);                                                                                       
        Assert.Equal("/usr/bin/test --arg", info.CmdLine);                                                                             
        Assert.Equal(1024, info.DiskUsage);                                                                                            
        Assert.Equal(100UL, info.DiskOperations);                                                                                      
        Assert.Equal(2048, info.UsedMemory);                                                                                           
        Assert.Equal(15.5, info.CpuTimePercent);                                                                                       
        Assert.Equal(10.2, info.CpuUserTimePercent);                                                                                   
        Assert.Equal(5.3, info.CpuKernelTimePercent);                                                                                  
        Assert.Equal(1000, info.PrevCpuKernelTime);                                                                                    
        Assert.Equal(2000, info.PrevCpuUserTime);                                                                                      
        Assert.Equal(1100, info.CurrCpuKernelTime);                                                                                    
        Assert.Equal(2200, info.CurrCpuUserTime);                                                                                      
    }                                                                                                                                  
                                                                                                                                         
    [Fact]                                                                                                                             
    public void Copy_Constructor_Creates_Deep_Copy()                                                                                   
    {                                                                                                                                  
        DateTime startTime = DateTime.Now;                                                                                             
        ProcessorInfo original = new() {                                                                                               
            Pid = 1234,                                                                                                                
            ThreadCount = 10,                                                                                                          
            HandleCount = 50,                                                                                                          
            BasePriority = 8,                                                                                                          
            ParentPid = 1,                                                                                                             
            IsDaemon = true,                                                                                                           
            IsLowPriority = false,                                                                                                     
            StartTime = startTime,                                                                                                     
            ProcessName = "test.exe",                                                                                                  
            FileDescription = "Test Process",                                                                                          
            UserName = "testuser",                                                                                                     
            CmdLine = "/usr/bin/test",                                                                                                 
            DiskUsage = 1024,                                                                                                          
            DiskOperations = 100,                                                                                                      
            UsedMemory = 2048,                                                                                                         
            CpuTimePercent = 15.5,                                                                                                     
            CpuUserTimePercent = 10.2,                                                                                                 
            CpuKernelTimePercent = 5.3,                                                                                                
            PrevCpuKernelTime = 1000,                                                                                                  
            PrevCpuUserTime = 2000,                                                                                                    
            CurrCpuKernelTime = 1100,                                                                                                  
            CurrCpuUserTime = 2200                                                                                                     
        };                                                                                                                             
                                                                                                                                     
        ProcessorInfo copy = new(original);                                                                                            
                                                                                                                                     
        Assert.NotSame(original, copy);                                                                                                
        Assert.Equal(original.Pid, copy.Pid);                                                                                          
        Assert.Equal(original.ThreadCount, copy.ThreadCount);                                                                          
        Assert.Equal(original.HandleCount, copy.HandleCount);                                                                          
        Assert.Equal(original.BasePriority, copy.BasePriority);                                                                        
        Assert.Equal(original.ParentPid, copy.ParentPid);                                                                              
        Assert.Equal(original.IsDaemon, copy.IsDaemon);                                                                                
        Assert.Equal(original.IsLowPriority, copy.IsLowPriority);                                                                      
        Assert.Equal(original.StartTime, copy.StartTime);                                                                              
        Assert.Equal(original.ProcessName, copy.ProcessName);                                                                          
        Assert.Equal(original.FileDescription, copy.FileDescription);                                                                  
        Assert.Equal(original.UserName, copy.UserName);                                                                                
        Assert.Equal(original.CmdLine, copy.CmdLine);                                                                                  
        Assert.Equal(original.DiskUsage, copy.DiskUsage);                                                                              
        Assert.Equal(original.DiskOperations, copy.DiskOperations);                                                                    
        Assert.Equal(original.UsedMemory, copy.UsedMemory);                                                                            
        Assert.Equal(original.CpuTimePercent, copy.CpuTimePercent);                                                                    
        Assert.Equal(original.CpuUserTimePercent, copy.CpuUserTimePercent);                                                            
        Assert.Equal(original.CpuKernelTimePercent, copy.CpuKernelTimePercent);                                                        
        Assert.Equal(original.PrevCpuKernelTime, copy.PrevCpuKernelTime);                                                              
        Assert.Equal(original.PrevCpuUserTime, copy.PrevCpuUserTime);                                                                  
        Assert.Equal(original.CurrCpuKernelTime, copy.CurrCpuKernelTime);                                                              
        Assert.Equal(original.CurrCpuUserTime, copy.CurrCpuUserTime);                                                                  
    }                                                                                                                                  
                                                                                                                                         
    [Fact]                                                                                                                             
    public void Copy_Constructor_Copy_Is_Independent_From_Original()                                                                   
    {                                                                                                                                  
        ProcessorInfo original = new() {                                                                                               
            Pid = 1234,                                                                                                                
            ProcessName = "original.exe",                                                                                              
            CpuTimePercent = 10.0                                                                                                      
        };                                                                                                                             
                                                                                                                                     
        ProcessorInfo copy = new(original);                                                                                            
                                                                                                                                     
        original.Pid = 5678;                                                                                                           
        original.ProcessName = "modified.exe";                                                                                         
        original.CpuTimePercent = 20.0;                                                                                                
                                                                                                                                     
        Assert.Equal(1234, copy.Pid);                                                                                                  
        Assert.Equal("original.exe", copy.ProcessName);                                                                                
        Assert.Equal(10.0, copy.CpuTimePercent);                                                                                       
    }                                                                                                                                  
    
    [Fact]                                                                                                                             
    public void GetHashCode_Is_Consistent()                                                                                            
    {                                                                                                                                  
        ProcessorInfo info = new() {                                                                                                   
            Pid = 1234,                                                                                                                
            ProcessName = "test.exe",                                                                                                  
            CpuTimePercent = 15.5                                                                                                      
        };                                                                                                                             
                                                                                                                                     
        int hash1 = info.GetHashCode();                                                                                                
        int hash2 = info.GetHashCode();                                                                                                
                                                                                                                                     
        Assert.Equal(hash1, hash2);                                                                                                    
    }                                                                                                                                  
                                                                                                                                         
    [Fact]                                                                                                                             
    public void GetHashCode_Is_Equal_For_Equal_Objects()                                                                               
    {                                                                                                                                  
        DateTime startTime = DateTime.Now;                                                                                             
        ProcessorInfo info1 = new() {                                                                                                  
            Pid = 1234,                                                                                                                
            ThreadCount = 10,                                                                                                          
            ProcessName = "test.exe",                                                                                                  
            StartTime = startTime,                                                                                                     
            CpuTimePercent = 15.5                                                                                                      
        };                                                                                                                             
                                                                                                                                     
        ProcessorInfo info2 = new() {                                                                                                  
            Pid = 1234,                                                                                                                
            ThreadCount = 10,                                                                                                          
            ProcessName = "test.exe",                                                                                                  
            StartTime = startTime,                                                                                                     
            CpuTimePercent = 15.5                                                                                                      
        };                                                                                                                             
                                                                                                                                     
        Assert.Equal(info1.GetHashCode(), info2.GetHashCode());                                                                        
    }                                                                                                                                  
                                                                                                                                         
    [Fact]                                                                                                                             
    public void GetHashCode_Is_Different_For_Different_Objects()                                                                       
    {                                                                                                                                  
        ProcessorInfo info1 = new() { Pid = 1234 };                                                                                    
        ProcessorInfo info2 = new() { Pid = 5678 };                                                                                    
                                                                                                                                     
        Assert.NotEqual(info1.GetHashCode(), info2.GetHashCode());                                                                     
    }                                                                                                                                  
      
    [Fact]                                                                                                                             
    public void Equals_Checks_All_Properties()                                                                                         
    {                                                                                                                                  
        ProcessorInfo info1 = new() {                                                                                                  
            Pid = 1234,                                                                                                                
            ThreadCount = 10,                                                                                                          
            HandleCount = 50,                                                                                                          
            BasePriority = 8,                                                                                                          
            ParentPid = 1,                                                                                                             
            IsDaemon = true,                                                                                                           
            IsLowPriority = false,                                                                                                     
            StartTime = DateTime.Now,                                                                                                  
            ProcessName = "test.exe",                                                                                                  
            FileDescription = "Test",                                                                                                  
            UserName = "user",                                                                                                         
            CmdLine = "/test",                                                                                                         
            DiskUsage = 100,                                                                                                           
            DiskOperations = 10,                                                                                                       
            UsedMemory = 1024,                                                                                                         
            CpuTimePercent = 15.0,                                                                                                     
            CpuUserTimePercent = 10.0,                                                                                                 
            CpuKernelTimePercent = 5.0,                                                                                                
            PrevCpuKernelTime = 1000,                                                                                                  
            PrevCpuUserTime = 2000,                                                                                                    
            CurrCpuKernelTime = 1100,                                                                                                  
            CurrCpuUserTime = 2100                                                                                                     
        };                                                                                                                             
                                                                                                                                     
        ProcessorInfo info2 = new(info1);                                                                                              
                                                                                                                                     
        Assert.True(info1.Equals(info2));                                                                                              
                                                                                                                                     
        info2.ThreadCount = 999;                                                                                                       
        Assert.False(info1.Equals(info2));                                                                                             
    }                                                                                                                                  
}
