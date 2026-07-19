using System.Runtime.InteropServices;
using MacroDeck.HardwareMonitor.Core.Models;

namespace MacroDeck.HardwareMonitor.Infrastructure.Providers;

public sealed class MemoryProvider
{
    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    private const double BytesToGb = 1024.0 * 1024.0 * 1024.0;

    public IReadOnlyList<SensorReading> GetReadings()
    {
        var state = new MEMORYSTATUSEX();
        state.dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();

        if (!GlobalMemoryStatusEx(ref state))
        {
            return Array.Empty<SensorReading>();
        }

        var totalGb = state.ullTotalPhys / BytesToGb;
        var availGb = state.ullAvailPhys / BytesToGb;
        var usedGb = totalGb - availGb;
        var percent = state.dwMemoryLoad;

        var now = DateTime.UtcNow;

        return new List<SensorReading>
        {
            new("ram.percent", "RAM Usage", SensorType.Load, percent, "%", now),
            new("ram.used.gb", "RAM Used", SensorType.Data, (float)Math.Round(usedGb, 1), "GB", now),
            new("ram.available.gb", "RAM Available", SensorType.Data, (float)Math.Round(availGb, 1), "GB", now),
            new("ram.total.gb", "RAM Total", SensorType.Data, (float)Math.Round(totalGb, 1), "GB", now)
        };
    }
}
