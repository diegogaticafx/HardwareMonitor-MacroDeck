using MacroDeck.HardwareMonitor.Core.Models;

namespace MacroDeck.HardwareMonitor.Tests;

public class HardwareSnapshotTests
{
    private static readonly DateTime Now = DateTime.UtcNow;

    [Fact]
    public void GetCpuUsage_ReturnsCorrectValue()
    {
        var sensors = new List<SensorReading>
        {
            new("cpu.load.cpu_total", "CPU Total", SensorType.Load, 45.5f, "%", Now)
        };
        var snapshot = new HardwareSnapshot(sensors, [], [], [], Now);
        Assert.Equal(45.5f, snapshot.GetCpuUsage());
    }

    [Fact]
    public void GetCpuUsage_WhenNoLoadSensor_ReturnsNull()
    {
        var sensors = new List<SensorReading>
        {
            new("cpu.temp.cpu_package", "CPU Package", SensorType.Temperature, 55.0f, "°C", Now)
        };
        var snapshot = new HardwareSnapshot(sensors, [], [], [], Now);
        Assert.Null(snapshot.GetCpuUsage());
    }

    [Fact]
    public void GetCpuTemperature_ReturnsCorrectValue()
    {
        var sensors = new List<SensorReading>
        {
            new("cpu.temperature.cpu_package", "CPU Package", SensorType.Temperature, 65.5f, "°C", Now)
        };
        var snapshot = new HardwareSnapshot(sensors, [], [], [], Now);
        Assert.Equal(65.5f, snapshot.GetCpuTemperature());
    }

    [Fact]
    public void GetGpuUsage_ReturnsCorrectValue()
    {
        var sensors = new List<SensorReading>
        {
            new("gpu.load.core", "GPU Core", SensorType.Load, 80.0f, "%", Now)
        };
        var snapshot = new HardwareSnapshot([], sensors, [], [], Now);
        Assert.Equal(80.0f, snapshot.GetGpuUsage());
    }

    [Fact]
    public void GetGpuTemperature_ReturnsCorrectValue()
    {
        var sensors = new List<SensorReading>
        {
            new("gpu.temperature.gpu_core", "GPU Core", SensorType.Temperature, 72.3f, "°C", Now)
        };
        var snapshot = new HardwareSnapshot([], sensors, [], [], Now);
        Assert.Equal(72.3f, snapshot.GetGpuTemperature());
    }

    [Fact]
    public void GetMemoryUsagePercent_ReturnsCorrectValue()
    {
        var sensors = new List<SensorReading>
        {
            new("ram.load.memory", "Memory", SensorType.Load, 65.0f, "%", Now)
        };
        var snapshot = new HardwareSnapshot([], [], sensors, [], Now);
        Assert.Equal(65.0f, snapshot.GetMemoryUsagePercent());
    }

    [Fact]
    public void Empty_ReturnsEmptySnaphot()
    {
        var empty = HardwareSnapshot.Empty;
        Assert.Empty(empty.CpuSensors);
        Assert.Empty(empty.GpuSensors);
        Assert.Empty(empty.MemorySensors);
        Assert.Empty(empty.StorageSensors);
        Assert.Equal(DateTime.MinValue, empty.Timestamp);
    }

    [Fact]
    public void Snapshot_StoresCorrectReadings()
    {
        var cpu = new List<SensorReading>
        {
            new("cpu.usage", "CPU Usage", SensorType.Load, 50f, "%", Now)
        };
        var gpu = new List<SensorReading>
        {
            new("gpu.usage", "GPU Usage", SensorType.Load, 60f, "%", Now)
        };
        var mem = new List<SensorReading>
        {
            new("ram.percent", "RAM", SensorType.Load, 40f, "%", Now)
        };
        var disk = new List<SensorReading>
        {
            new("disk.c.percent", "Disk C:", SensorType.Load, 30f, "%", Now)
        };

        var snapshot = new HardwareSnapshot(cpu, gpu, mem, disk, Now);
        Assert.Equal(50f, snapshot.GetCpuUsage());
        Assert.Equal(60f, snapshot.GetGpuUsage());
        Assert.Equal(40f, snapshot.GetMemoryUsagePercent());
        Assert.Equal(30f, snapshot.GetDiskUsagePercent("c"));
    }
}
