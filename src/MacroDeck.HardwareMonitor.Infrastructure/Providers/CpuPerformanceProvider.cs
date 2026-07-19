using System.Runtime.InteropServices;
using MacroDeck.HardwareMonitor.Core.Models;

namespace MacroDeck.HardwareMonitor.Infrastructure.Providers;

public sealed class CpuPerformanceProvider : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    private struct FILETIME
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
        public long ToLong() => ((long)dwHighDateTime << 32) | dwLowDateTime;
    }

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetSystemTimes(out FILETIME idleTime, out FILETIME kernelTime, out FILETIME userTime);

    private long _prevIdle;
    private long _prevKernel;
    private long _prevUser;
    private bool _hasPrev;

    public SensorReading GetUsage()
    {
        if (!GetSystemTimes(out var idle, out var kernel, out var user))
        {
            return new SensorReading("cpu.usage", "CPU Usage", SensorType.Load, null, "%", DateTime.UtcNow);
        }

        var idleTicks = idle.ToLong();
        var kernelTicks = kernel.ToLong();
        var userTicks = user.ToLong();

        if (!_hasPrev)
        {
            _prevIdle = idleTicks;
            _prevKernel = kernelTicks;
            _prevUser = userTicks;
            _hasPrev = true;
            return new SensorReading("cpu.usage", "CPU Usage", SensorType.Load, 0f, "%", DateTime.UtcNow);
        }

        var idleDiff = idleTicks - _prevIdle;
        var kernelDiff = kernelTicks - _prevKernel;
        var userDiff = userTicks - _prevUser;

        _prevIdle = idleTicks;
        _prevKernel = kernelTicks;
        _prevUser = userTicks;

        var totalDiff = kernelDiff + userDiff;
        if (totalDiff == 0)
        {
            return new SensorReading("cpu.usage", "CPU Usage", SensorType.Load, 0f, "%", DateTime.UtcNow);
        }

        var usage = 100.0f - (idleDiff * 100.0f / totalDiff);
        usage = Math.Clamp(usage, 0f, 100f);

        return new SensorReading("cpu.usage", "CPU Usage", SensorType.Load, usage, "%", DateTime.UtcNow);
    }

    public void Dispose()
    {
        _hasPrev = false;
    }
}
