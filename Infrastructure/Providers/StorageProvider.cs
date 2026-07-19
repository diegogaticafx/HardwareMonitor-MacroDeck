using System.Diagnostics;
using MacroDeck.HardwareMonitor.Core.Models;

namespace MacroDeck.HardwareMonitor.Infrastructure.Providers;

public sealed class StorageProvider : IDisposable
{
    private const double BytesToGb = 1024.0 * 1024.0 * 1024.0;
    private readonly Dictionary<string, PerformanceCounter?> _activityCounters = new();
    private bool _disposed;

    public IReadOnlyList<SensorReading> GetReadings()
    {
        var result = new List<SensorReading>();
        var now = DateTime.UtcNow;

        try
        {
            var drives = DriveInfo.GetDrives();
            for (int i = 0; i < drives.Length; i++)
            {
                var drive = drives[i];
                if (!drive.IsReady || drive.DriveType != DriveType.Fixed) continue;

                var letter = drive.Name[0].ToString().ToLowerInvariant();
                var totalGb = drive.TotalSize / BytesToGb;
                var freeGb = drive.AvailableFreeSpace / BytesToGb;
                var usedGb = totalGb - freeGb;
                var usedPercent = totalGb > 0 ? (float)(usedGb / totalGb * 100.0) : 0f;
                var activity = GetDiskActivity(letter);

                result.Add(new SensorReading($"disk.{letter}.activity", $"Disk {letter}: Activity", SensorType.Load, activity, "%", now));
                result.Add(new SensorReading($"disk.{letter}.used.percent", $"Disk {letter}: Used Space", SensorType.Load, usedPercent, "%", now));
                result.Add(new SensorReading($"disk.{letter}.total.gb", $"Disk {letter}: Total", SensorType.Data, (float)Math.Round(totalGb, 1), "GB", now));
                result.Add(new SensorReading($"disk.{letter}.used.gb", $"Disk {letter}: Used", SensorType.Data, (float)Math.Round(usedGb, 1), "GB", now));
                result.Add(new SensorReading($"disk.{letter}.free.gb", $"Disk {letter}: Free", SensorType.Data, (float)Math.Round(freeGb, 1), "GB", now));
            }
        }
        catch { }

        return result;
    }

    private float GetDiskActivity(string driveLetter)
    {
        try
        {
            if (!_activityCounters.TryGetValue(driveLetter, out var counter))
            {
                var instance = FindPhysicalDiskInstance(driveLetter);
                if (instance is null)
                {
                    _activityCounters[driveLetter] = null;
                    return 0;
                }
                counter = new PerformanceCounter("PhysicalDisk", "% Idle Time", instance);
                counter.NextValue();
                _activityCounters[driveLetter] = counter;
            }
            if (counter is null) return 0;
            return Math.Max(0, Math.Min(100, 100f - counter.NextValue()));
        }
        catch
        {
            return 0;
        }
    }

    private static string? FindPhysicalDiskInstance(string driveLetter)
    {
        try
        {
            var category = new PerformanceCounterCategory("PhysicalDisk");
            return category.GetInstanceNames()
                .FirstOrDefault(i => i.Contains($"{driveLetter}:", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return null;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var counter in _activityCounters.Values)
            counter?.Dispose();
        _activityCounters.Clear();
    }
}
