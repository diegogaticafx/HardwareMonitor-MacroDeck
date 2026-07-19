using MacroDeck.HardwareMonitor.Core.Models;

namespace MacroDeck.HardwareMonitor.Tests;

public class SensorReadingTests
{
    [Fact]
    public void FormattedValue_WhenNull_ReturnsN_A()
    {
        var reading = new SensorReading("test", "Test", SensorType.Temperature, null, "°C", DateTime.UtcNow);
        Assert.Equal("N/A", reading.FormattedValue);
    }

    [Fact]
    public void FormattedValue_Temperature_FormatsCorrectly()
    {
        var reading = new SensorReading("test", "Test", SensorType.Temperature, 45.5f, "°C", DateTime.UtcNow);
        Assert.Equal("45.5°C", reading.FormattedValue);
    }

    [Fact]
    public void FormattedValue_Load_FormatsCorrectly()
    {
        var reading = new SensorReading("cpu.usage", "CPU Usage", SensorType.Load, 75.2f, "%", DateTime.UtcNow);
        Assert.Equal("75.2%", reading.FormattedValue);
    }

    [Fact]
    public void FormattedValue_Clock_FormatsCorrectly()
    {
        var reading = new SensorReading("cpu.clock", "CPU Clock", SensorType.Clock, 3500f, "MHz", DateTime.UtcNow);
        Assert.Equal("3500 MHz", reading.FormattedValue);
    }

    [Fact]
    public void FormattedValue_Power_FormatsCorrectly()
    {
        var reading = new SensorReading("cpu.power", "CPU Power", SensorType.Power, 65.5f, "W", DateTime.UtcNow);
        Assert.Equal("65.5 W", reading.FormattedValue);
    }

    [Fact]
    public void FormattedValue_Voltage_FormatsCorrectly()
    {
        var reading = new SensorReading("cpu.vid", "CPU VID", SensorType.Voltage, 1.200f, "V", DateTime.UtcNow);
        Assert.Equal("1.200 V", reading.FormattedValue);
    }

    [Fact]
    public void FormattedValue_Fan_FormatsCorrectly()
    {
        var reading = new SensorReading("fan.cpu", "CPU Fan", SensorType.Fan, 1200f, "RPM", DateTime.UtcNow);
        Assert.Equal("1200 RPM", reading.FormattedValue);
    }

    [Fact]
    public void IsAvailable_WhenValueIsNull_ReturnsFalse()
    {
        var reading = new SensorReading("test", "Test", SensorType.Temperature, null, "°C", DateTime.UtcNow);
        Assert.False(reading.IsAvailable);
    }

    [Fact]
    public void IsAvailable_WhenValueIsSet_ReturnsTrue()
    {
        var reading = new SensorReading("test", "Test", SensorType.Temperature, 36.6f, "°C", DateTime.UtcNow);
        Assert.True(reading.IsAvailable);
    }
}
