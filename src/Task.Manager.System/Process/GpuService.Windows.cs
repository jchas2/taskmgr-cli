using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Task.Manager.Interop.Win32;

namespace Task.Manager.System.Process;

public static partial class GpuService
{
#if __WIN32__
#pragma warning disable CA1416

    private static Dictionary<int, long> GetProcessStatsInternal()
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
            
            int structSize = Marshal.SizeOf<Pdh.PDH_RAW_COUNTER_ITEM>();
            
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
    
    private static int ParsePidFromInstance(string instanceName)
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