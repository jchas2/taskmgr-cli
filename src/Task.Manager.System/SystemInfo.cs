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
        var activeNics = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType == networkInterfaceType)
            .ToList();

        foreach (var nic in activeNics) {
            foreach (var ipInfo in nic.GetIPProperties().UnicastAddresses) {
                if (ipInfo.Address.AddressFamily == AddressFamily.InterNetwork) {
                    yield return ipInfo.Address;
                }
            }
        }
    }

    public bool GetSystemStatistics(ref SystemStatistics stats)
    {
        stats.MachineName = Environment.MachineName;
        
        stats.CpuCores = (ulong)Environment.ProcessorCount;
        GetCpuInfoInternal(ref stats);

        var ip = GetPreferredIpAddress();
        stats.PrivateIPv4Address = ip == null 
            ? string.Empty 
            : ip.ToString();
        
        stats.OsVersion = Environment.OSVersion.VersionString;
        
        return true;
    }
    
    public bool IsRunningAsRoot() => IsRunningAsRootInternal();
}