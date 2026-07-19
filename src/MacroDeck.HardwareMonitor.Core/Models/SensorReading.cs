namespace MacroDeck.HardwareMonitor.Core.Models;

public readonly record struct SensorReading(
    string Identifier,
    string Name,
    SensorType Type,
    float? Value,
    string Unit,
    DateTime Timestamp)
{
    public bool IsAvailable => Value.HasValue;

    public string FormattedValue => Value.HasValue
        ? Type switch
        {
            SensorType.Temperature => $"{Value.Value:F1}°C",
            SensorType.Load => $"{Value.Value:F1}%",
            SensorType.Clock => $"{Value.Value:F0} MHz",
            SensorType.Power => $"{Value.Value:F1} W",
            SensorType.Voltage => $"{Value.Value:F3} V",
            SensorType.Fan => $"{Value.Value:F0} RPM",
            SensorType.Data => FormatDataValue(Value.Value),
            _ => $"{Value.Value:F2} {Unit}"
        }
        : "N/A";

    private static string FormatDataValue(float value)
    {
        if (value >= 1_000_000_000) return $"{value / 1_000_000_000:F2} GB";
        if (value >= 1_000_000) return $"{value / 1_000_000:F2} MB";
        if (value >= 1_000) return $"{value / 1_000:F2} KB";
        return $"{value:F0} B";
    }
}
