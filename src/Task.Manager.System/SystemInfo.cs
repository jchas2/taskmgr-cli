using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Task.Manager.System;

public partial class SystemInfo : ISystemInfo
{
    public bool GetCpuInfo(ref SystemStatistics systemStatistics) => GetCpuInfoInternal(ref systemStatistics);
    public bool GetCpuTimes(ref SystemTimes systemTimes) => GetCpuTimesInternal(ref systemTimes);

    public IPAddress? GetPreferredIpAddress()
    {
        List<IPAddress> ipAddresses = GetPreferredIpAddressesInternal(NetworkInterfaceType.Ethernet).ToList();
        
        if (ipAddresses.Any()) {
            return ipAddresses.First();
        }

        ipAddresses = GetPreferredIpAddressesInternal(NetworkInterfaceType.Wireless80211).ToList();
        
        if (ipAddresses.Any()) {
            return ipAddresses.First();
        }

        return null;
    }

    private IEnumerable<IPAddress> GetPreferredIpAddressesInternal(NetworkInterfaceType networkInterfaceType)
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
    
    public bool GetSystemStatistics(ref SystemStatistics systemStatistics) => GetSystemStatisticsInternal(ref systemStatistics);
    public bool IsRunningAsRoot() => IsRunningAsRootInternal();

}