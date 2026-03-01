using Task.Manager.System;
using Task.Manager.System.Process;

class TestNetwork
{
    static void Main()
    {
        var networkService = new NetworkService();
        var stats = new SystemStatistics();

        bool success = networkService.GetNetworkStats(ref stats);

        Console.WriteLine($"Network stats collection: {(success ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"Bytes Sent:     {stats.NetworkBytesSent:N0}");
        Console.WriteLine($"Bytes Received: {stats.NetworkBytesReceived:N0}");
        Console.WriteLine($"Packets Sent:     {stats.NetworkPacketsSent:N0}");
        Console.WriteLine($"Packets Received: {stats.NetworkPacketsReceived:N0}");
        Console.WriteLine();

        // Convert to human-readable
        Console.WriteLine($"Total Sent:     {FormatBytes(stats.NetworkBytesSent)}");
        Console.WriteLine($"Total Received: {FormatBytes(stats.NetworkBytesReceived)}");
    }

    static string FormatBytes(ulong bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
