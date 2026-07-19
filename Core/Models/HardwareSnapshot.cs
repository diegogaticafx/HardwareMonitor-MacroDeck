namespace MacroDeck.HardwareMonitor.Core.Models;

public readonly record struct HardwareSnapshot(
    IReadOnlyList<SensorReading> CpuSensors,
    IReadOnlyList<SensorReading> GpuSensors,
    IReadOnlyList<SensorReading> MemorySensors,
    IReadOnlyList<SensorReading> StorageSensors,
    DateTime Timestamp)
{
    public static readonly HardwareSnapshot Empty = new(
        Array.Empty<SensorReading>(),
        Array.Empty<SensorReading>(),
        Array.Empty<SensorReading>(),
        Array.Empty<SensorReading>(),
        DateTime.MinValue);

    public float? GetCpuUsage()
        => FindFirst(CpuSensors, s => s.Type == SensorType.Load && s.Identifier.Contains("cpu_total", StringComparison.OrdinalIgnoreCase))
        ?? FindFirst(CpuSensors, s => s.Type == SensorType.Load)
        ?? FindFirst(CpuSensors, s => s.Type == SensorType.Load && s.Name.Contains("total", StringComparison.OrdinalIgnoreCase));

    public float? GetCpuTemperature()
        => FindFirst(CpuSensors, s => s.Type == SensorType.Temperature && s.Name.Contains("package", StringComparison.OrdinalIgnoreCase))
        ?? FindFirst(CpuSensors, s => s.Type == SensorType.Temperature && s.Name.Contains("cpu", StringComparison.OrdinalIgnoreCase))
        ?? FindFirst(CpuSensors, s => s.Type == SensorType.Temperature);

    public float? GetGpuUsage()
        => FindFirst(GpuSensors, s => s.Type == SensorType.Load && s.Name.Contains("core", StringComparison.OrdinalIgnoreCase))
        ?? FindFirst(GpuSensors, s => s.Type == SensorType.Load && s.Name.Contains("gpu", StringComparison.OrdinalIgnoreCase))
        ?? FindFirst(GpuSensors, s => s.Type == SensorType.Load);

    public float? GetGpuTemperature()
        => FindFirst(GpuSensors, s => s.Type == SensorType.Temperature);

    public float? GetMemoryUsagePercent()
        => FindFirst(MemorySensors, s => s.Type == SensorType.Load);

    public float? GetMemoryUsedGb()
        => FindFirst(MemorySensors, s => s.Identifier.Contains("used", StringComparison.OrdinalIgnoreCase));

    public float? GetMemoryFreeGb()
        => FindFirst(MemorySensors, s => s.Identifier.Contains("available", StringComparison.OrdinalIgnoreCase))
        ?? FindFirst(MemorySensors, s => s.Identifier.Contains("free", StringComparison.OrdinalIgnoreCase));

    public float? GetMemoryTotalGb()
        => FindFirst(MemorySensors, s => s.Identifier.Contains("total", StringComparison.OrdinalIgnoreCase));

    public float? GetDiskUsagePercent(string drive = "c")
        => FindFirst(StorageSensors, s => s.Type == SensorType.Load && s.Identifier.StartsWith($"disk.{drive}.", StringComparison.OrdinalIgnoreCase));

    public float? GetDiskTemperature(string drive = "c")
        => FindFirst(StorageSensors, s => s.Type == SensorType.Temperature && s.Identifier.StartsWith($"disk.{drive}.", StringComparison.OrdinalIgnoreCase));

    private static float? FindFirst(IReadOnlyList<SensorReading> sensors, Func<SensorReading, bool> predicate)
    {
        for (int i = 0; i < sensors.Count; i++)
        {
            var s = sensors[i];
            if (predicate(s) && s.Value.HasValue)
                return s.Value.Value;
        }
        return null;
    }
}
