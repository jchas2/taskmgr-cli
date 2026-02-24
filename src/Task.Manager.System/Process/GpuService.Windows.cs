using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Task.Manager.Interop.Win32;

namespace Task.Manager.System.Process;

public partial class GpuService
{
#if __WIN32__
#pragma warning disable CA1416
    private static bool GetGpuMemoryInternal(ref SystemStatistics systemStatistics)
    {
        bool result = true;

        if (systemStatistics.TotalGpuMemory == 0) {
            result = GetGpuMemoryInternalTotal(ref systemStatistics);
        }
        
        systemStatistics.AvailableGpuMemory = 0;
        return result && GetGpuMemoryInternalUsed(ref systemStatistics);
    }

    private static bool GetGpuMemoryInternalTotal(ref SystemStatistics systemStatistics)
    {
        // NVIDIA + AMD GPUs store memory info in the display adapter key.
        const string RegPath = @"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}";
        const string RegKeyMemorySize = "HardwareInformation.qwMemorySize";
        
        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(RegPath);
        
        if (key == null) {
            return false;
        }                

        // Subkey for each adapter (0000, 0001, 0002 etc).
        foreach (var subKeyName in key.GetSubKeyNames())
        {
            if (!subKeyName.StartsWith("0")) {
                continue;
            }

            using RegistryKey? subKey = key.OpenSubKey(subKeyName);
            object? memorySize = subKey?.GetValue(RegKeyMemorySize);

            if (memorySize == null) {
                continue;
            }
            
            // REG_QWORD.
            if (memorySize is long memLong && memLong > 0) {
                systemStatistics.TotalGpuMemory += memLong;
            }
            else if (memorySize is ulong memULong && memULong > 0) {
                systemStatistics.TotalGpuMemory += (long)memULong;
            }
            else if (memorySize is byte[] bytes && bytes.Length == 8) {
                long memory = BitConverter.ToInt64(bytes, 0);
                systemStatistics.TotalGpuMemory += memory;
            }
        }
        
        return true;
    }
    
    private static bool GetGpuMemoryInternalUsed(ref SystemStatistics systemStatistics)
    {
        const string AdapterMemoryCounterPath = @"\GPU Adapter Memory(*)\Dedicated Usage";
        
        IntPtr hQuery;
        IntPtr hCounter;

        if (Pdh.PdhOpenQuery(
            null,
            IntPtr.Zero,
            out hQuery) != Pdh.ERROR_SUCCESS) {

            return false;
        }

        Pdh.PdhAddEnglishCounter(
            hQuery, 
            AdapterMemoryCounterPath, 
            IntPtr.Zero, 
            out hCounter);
        
        Pdh.PdhCollectQueryData(hQuery);

        uint bufferSize = 0;
        uint itemCount = 0;
        long usedDedicatedMemory = 0;

        Pdh.PdhGetRawCounterArray(hCounter, ref bufferSize, ref itemCount, IntPtr.Zero);

        if (bufferSize <= 0) {
            Pdh.PdhCloseQuery(hQuery);
            return false;
        }

        IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);

        if (Pdh.PdhGetRawCounterArray(
            hCounter, 
            ref bufferSize, 
            ref itemCount, 
            buffer) == Pdh.ERROR_SUCCESS) {
        
            int structSize = Marshal.SizeOf(typeof(Pdh.PDH_RAW_COUNTER_ITEM));
            
            for (int i = 0; i < itemCount; i++) {
                IntPtr currentItemPtr = new IntPtr(buffer.ToInt64() + (i * structSize));
                Pdh.PDH_RAW_COUNTER_ITEM item = Marshal.PtrToStructure<Pdh.PDH_RAW_COUNTER_ITEM>(currentItemPtr);

                if (item.RawValue.CStatus == Pdh.PDH_CSTATUS_VALID_DATA) {
                    usedDedicatedMemory += item.RawValue.FirstValue;
                }
            }
        }
        
        Marshal.FreeHGlobal(buffer);
        Pdh.PdhCloseQuery(hQuery);
        
        systemStatistics.AvailableGpuMemory = systemStatistics.TotalGpuMemory - usedDedicatedMemory;
        return true;        
    }

    private Dictionary<int, long> GetProcessStatsInternal()
    {
        const string GpuEngineCounterPath = @"\GPU Engine(*)\Running Time";
        
        Dictionary<int, long> gpuInfo = new();
        IntPtr hQuery;
        IntPtr hCounter;

        if (Pdh.PdhOpenQuery(
            null,
            IntPtr.Zero,
            out hQuery) != Pdh.ERROR_SUCCESS) {

            return gpuInfo;
        }

        Pdh.PdhAddEnglishCounter(
            hQuery, 
            GpuEngineCounterPath, 
            IntPtr.Zero, 
            out hCounter);
            
        // Collect once to hydrate the array.
        Pdh.PdhCollectQueryData(hQuery);

        uint bufferSize = 0;
        uint itemCount = 0;

        Pdh.PdhGetRawCounterArray(
            hCounter, 
            ref bufferSize, 
            ref itemCount, 
            IntPtr.Zero);

        if (bufferSize <= 0) {
            Pdh.PdhCloseQuery(hQuery);
            return gpuInfo;
        }
        
        IntPtr buffer = Marshal.AllocHGlobal((int)bufferSize);
        
        if (Pdh.PdhGetRawCounterArray(
                hCounter, 
                ref bufferSize, 
                ref itemCount, 
                buffer) == Pdh.ERROR_SUCCESS) {
            
            int structSize = Marshal.SizeOf(typeof(Pdh.PDH_RAW_COUNTER_ITEM));
            
            for (int i = 0; i < itemCount; i++) {
                IntPtr currentItemPtr = new IntPtr(buffer.ToInt64() + (i * structSize));
                Pdh.PDH_RAW_COUNTER_ITEM item = Marshal.PtrToStructure<Pdh.PDH_RAW_COUNTER_ITEM>(currentItemPtr);

                if (item.RawValue.CStatus == Pdh.PDH_CSTATUS_VALID_DATA && item.RawValue.FirstValue > 0) {
                    int strLen = item.szName.Length;
                    
                    if (!(item.szName.Length >= 12 &&
                        item.szName[strLen - 1] == 'D' &&
                        item.szName[strLen - 2] == '3' &&
                        item.szName[strLen - 3] == '_' &&
                        item.szName[strLen - 4] == 'e' &&
                        item.szName[strLen - 5] == 'p' &&
                        item.szName[strLen - 6] == 'y' &&
                        item.szName[strLen - 7] == 't' &&
                        item.szName[strLen - 8] == 'g' &&
                        item.szName[strLen - 9] == 'n' &&
                        item.szName[strLen - 10] == 'e' &&
                        item.szName[strLen - 11] == '_')) {
                        
                        continue;
                    }
                    
                    int pid = ParsePidFromInstance(item.szName);
                    long result = item.RawValue.FirstValue;
                    
                    if (!gpuInfo.ContainsKey(pid)) {
                        gpuInfo[pid] = 0;
                    }
                    
                    gpuInfo[pid] += result;
                }
            }
        }
        
        Marshal.FreeHGlobal(buffer);
        Pdh.PdhCloseQuery(hQuery);
        
        return gpuInfo;
    }
    
    private int ParsePidFromInstance(string instanceName)
    {
        // Format example: pid_1234_luid_0x00000000_phys_0_eng_3_engtype_3D
        const string pidPrefix = "pid_";
        int pidIndex = instanceName.IndexOf(pidPrefix, StringComparison.OrdinalIgnoreCase);

        if (pidIndex == -1)
            return -1;

        int startIndex = pidIndex + pidPrefix.Length;
        if (startIndex >= instanceName.Length)
            return -1;

        int endIndex = instanceName.IndexOf('_', startIndex);
        if (endIndex == -1)
            endIndex = instanceName.Length;

        ReadOnlySpan<char> pidSpan = instanceName.AsSpan(startIndex, endIndex - startIndex);

        if (int.TryParse(pidSpan, out int pid))
            return pid;

        return -1;
    }

#pragma warning restore CA1416
#endif
}