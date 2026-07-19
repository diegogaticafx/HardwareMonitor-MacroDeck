using MacroDeck.HardwareMonitor.Core.Models;
using MacroDeck.HardwareMonitor.Infrastructure.Cache;

namespace MacroDeck.HardwareMonitor.Tests;

public class SensorCacheTests
{
    [Fact]
    public void GetSnapshot_Initially_ReturnsEmpty()
    {
        var cache = new SensorCache();
        var snapshot = cache.GetSnapshot();
        Assert.Equal(DateTime.MinValue, snapshot.Timestamp);
    }

    [Fact]
    public void UpdateSnapshot_StoresAndRetrievesValues()
    {
        var cache = new SensorCache();
        var now = DateTime.UtcNow;
        var sensors = new List<SensorReading>
        {
            new("cpu.usage", "CPU Usage", SensorType.Load, 50f, "%", now)
        };
        var snapshot = new HardwareSnapshot(sensors, [], [], [], now);
        cache.UpdateSnapshot(snapshot);

        var retrieved = cache.GetSnapshot();
        Assert.Equal(50f, retrieved.GetCpuUsage());
        Assert.Equal(now, retrieved.Timestamp);
    }

    [Fact]
    public void GetValue_Returns_CorrectValue()
    {
        var cache = new SensorCache();
        var now = DateTime.UtcNow;
        var sensors = new List<SensorReading>
        {
            new("cpu.temp", "CPU Temp", SensorType.Temperature, 45.2f, "°C", now)
        };
        var snapshot = new HardwareSnapshot(sensors, [], [], [], now);
        cache.UpdateSnapshot(snapshot);

        var value = cache.GetValue("cpu.temp");
        Assert.Equal(45.2f, value);
    }

    [Fact]
    public void GetValue_UnknownIdentifier_ReturnsNull()
    {
        var cache = new SensorCache();
        var value = cache.GetValue("nonexistent");
        Assert.Null(value);
    }

    [Fact]
    public void TryGetValue_ExistingKey_ReturnsTrue()
    {
        var cache = new SensorCache();
        var now = DateTime.UtcNow;
        var sensors = new List<SensorReading>
        {
            new("gpu.usage", "GPU Usage", SensorType.Load, 80f, "%", now)
        };
        var snapshot = new HardwareSnapshot([], sensors, [], [], now);
        cache.UpdateSnapshot(snapshot);

        Assert.True(cache.TryGetValue("gpu.usage", out var value));
        Assert.Equal(80f, value);
    }

    [Fact]
    public void TryGetValue_NonExistentKey_ReturnsFalse()
    {
        var cache = new SensorCache();
        Assert.False(cache.TryGetValue("missing", out _));
    }

    [Fact]
    public void UpdateSnapshot_ReplacesOldValues()
    {
        var cache = new SensorCache();
        var now = DateTime.UtcNow;

        var oldSnapshot = new HardwareSnapshot(
            [new SensorReading("cpu.usage", "CPU", SensorType.Load, 30f, "%", now)], [], [], [], now);
        cache.UpdateSnapshot(oldSnapshot);

        var newSnapshot = new HardwareSnapshot(
            [new SensorReading("cpu.usage", "CPU", SensorType.Load, 70f, "%", now)], [], [], [], now);
        cache.UpdateSnapshot(newSnapshot);

        Assert.Equal(70f, cache.GetValue("cpu.usage"));
    }
}
