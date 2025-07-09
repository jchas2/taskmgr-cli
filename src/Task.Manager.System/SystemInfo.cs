using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Task.Manager.System;

public partial class SystemInfo : ISystemInfo
{
    public bool GetCpuTimes(ref SystemTimes systemTimes) => GetCpuTimesInternal(ref systemTimes);

    private IPAddress? GetPreferredIpAddress()
    {
        List<IPAddress> ipAddresses = GetIpAddresses(NetworkInterfaceType.Ethernet).ToList();
        
        if (ipAddresses.Any()) {
            return ipAddresses.First();
        }

        ipAddresses = GetIpAddresses(NetworkInterfaceType.Wireless80211).ToList();
        
        if (ipAddresses.Any()) {
            return ipAddresses.First();
        }

        return null;
    }

    private IEnumerable<IPAddress> GetIpAddresses(NetworkInterfaceType networkInterfaceType)
    {
        List<NetworkInterface> activeNics = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType == networkInterfaceType)
            .ToList();

        foreach (NetworkInterface nic in activeNics) {
            foreach (UnicastIPAddressInformation ipInfo in nic.GetIPProperties().UnicastAddresses) {
                if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork) {
                    yield return ipInfo.Address;
                }
            }
        }
    }

    public bool GetSystemMemory(ref SystemStatistics systemStatistics) => GetSystemMemoryInternal(ref systemStatistics);

    public bool GetSystemInfo(ref SystemStatistics systemStatistics)
    {
        systemStatistics.MachineName = Environment.MachineName;
        systemStatistics.CpuCores = (ulong)Environment.ProcessorCount;
        
        bool result = GetCpuInfoInternal(ref systemStatistics);

        IPAddress? ip = GetPreferredIpAddress();
        
        /* With no Nic in an operational status the ip returned can be null. */
        systemStatistics.PrivateIPv4Address = ip == null 
            ? string.Empty 
            : ip.ToString();
        
        systemStatistics.OsVersion = Environment.OSVersion.VersionString;
        
        return result;
    }
    
    public bool IsRunningAsRoot() => IsRunningAsRootInternal();
}
