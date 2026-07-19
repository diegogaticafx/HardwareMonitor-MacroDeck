using MacroDeck.HardwareMonitor.Core.Models;
using SuchByte.MacroDeck.Plugins;
using SuchByte.MacroDeck.Variables;

namespace MacroDeck.HardwareMonitor;

internal static class VariableRegistrar
{
    private static readonly string[] SuggestionCategories =
    [
        "cpu.usage", "cpu.temp",
        "gpu.usage", "gpu.temp",
        "ram.usage", "ram.used", "ram.free", "ram.total", "ram.summary",
        "disk.c.load", "disk.c.usage", "disk.c.used", "disk.c.free", "disk.c.total", "disk.c.summary"
    ];

    public static void UpdateAll(HardwareSnapshot snapshot, MacroDeckPlugin plugin)
    {
        SetCpuVariables(snapshot, plugin);
        SetGpuVariables(snapshot, plugin);
        SetMemoryVariables(snapshot, plugin);
        SetStorageVariables(snapshot, plugin);
    }

    private static void SetValue(string name, object value, VariableType type, MacroDeckPlugin plugin)
    {
        VariableManager.SetValue(name, value, type, plugin, SuggestionCategories);
    }

    private static void SetCpuVariables(HardwareSnapshot snapshot, MacroDeckPlugin plugin)
    {
        var usage = snapshot.GetCpuUsage();
        var temp = snapshot.GetCpuTemperature();

        if (usage.HasValue)
            SetValue("cpu.usage", $"{usage.Value:F1}%", VariableType.String, plugin);
        if (temp.HasValue && temp.Value > 0f)
            SetValue("cpu.temp", $"{temp.Value:F1}°C", VariableType.String, plugin);
    }

    private static float _smoothGpuUsage = -1f;

    private static void SetGpuVariables(HardwareSnapshot snapshot, MacroDeckPlugin plugin)
    {
        var raw = snapshot.GetGpuUsage();
        var temp = snapshot.GetGpuTemperature();

        if (raw.HasValue)
        {
            if (_smoothGpuUsage < 0)
                _smoothGpuUsage = raw.Value;
            else
                _smoothGpuUsage = 0.3f * raw.Value + 0.7f * _smoothGpuUsage;

            SetValue("gpu.usage", $"{_smoothGpuUsage:F1}%", VariableType.String, plugin);
        }
        if (temp.HasValue && temp.Value > 0f)
            SetValue("gpu.temp", $"{temp.Value:F1}°C", VariableType.String, plugin);
    }

    private static void SetMemoryVariables(HardwareSnapshot snapshot, MacroDeckPlugin plugin)
    {
        var percent = snapshot.GetMemoryUsagePercent();
        var usedGb = snapshot.GetMemoryUsedGb();
        var freeGb = snapshot.GetMemoryFreeGb();
        var totalGb = snapshot.GetMemoryTotalGb();

        if (percent.HasValue)
            SetValue("ram.usage", $"{percent.Value:F1}%", VariableType.String, plugin);
        if (usedGb.HasValue)
            SetValue("ram.used", $"{(float)Math.Round(usedGb.Value, 1):F1} GB", VariableType.String, plugin);
        if (freeGb.HasValue)
            SetValue("ram.free", $"{(float)Math.Round(freeGb.Value, 1):F1} GB", VariableType.String, plugin);
        if (totalGb.HasValue)
            SetValue("ram.total", $"{(float)Math.Round(totalGb.Value, 1):F1} GB", VariableType.String, plugin);

        var summary = usedGb.HasValue && totalGb.HasValue
            ? $"{usedGb.Value:F1} / {totalGb.Value:F1} GB"
            : "N/A";
        SetValue("ram.summary", summary, VariableType.String, plugin);
    }

    private static void SetStorageVariables(HardwareSnapshot snapshot, MacroDeckPlugin plugin)
    {
        var processed = new HashSet<string>();

        for (int i = 0; i < snapshot.StorageSensors.Count; i++)
        {
            var sensor = snapshot.StorageSensors[i];
            if (sensor.Identifier.StartsWith("disk.", StringComparison.OrdinalIgnoreCase))
            {
                var parts = sensor.Identifier.Split('.');
                if (parts.Length > 1 && parts[1].Length == 1 && char.IsLetter(parts[1][0]) && !processed.Contains(parts[1]))
                {
                    processed.Add(parts[1]);
                    ProcessDrive(snapshot, parts[1], plugin);
                }
            }
        }

        if (processed.Count == 0)
            ProcessDrive(snapshot, "c", plugin);
    }

    private static void ProcessDrive(HardwareSnapshot snapshot, string driveLetter, MacroDeckPlugin plugin)
    {
        var prefix = $"disk.{driveLetter}";

        var activity = snapshot.GetDiskUsagePercent(driveLetter);
        var temp = snapshot.GetDiskTemperature(driveLetter);

        if (activity.HasValue)
            SetValue($"{prefix}.load", $"{activity.Value:F1}%", VariableType.String, plugin);
        if (temp.HasValue && temp.Value > 0f)
            SetValue($"{prefix}.temp", $"{temp.Value:F1}°C", VariableType.String, plugin);

        float? usedGb = null, freeGb = null, totalGb = null;
        for (int i = 0; i < snapshot.StorageSensors.Count; i++)
        {
            var s = snapshot.StorageSensors[i];
            if (s.Identifier == $"{prefix}.used.gb") usedGb = s.Value;
            if (s.Identifier == $"{prefix}.free.gb") freeGb = s.Value;
            if (s.Identifier == $"{prefix}.total.gb") totalGb = s.Value;
        }

        if (usedGb.HasValue)
            SetValue($"{prefix}.used", $"{(float)Math.Round(usedGb.Value, 1):F1} GB", VariableType.String, plugin);
        if (freeGb.HasValue)
            SetValue($"{prefix}.free", $"{(float)Math.Round(freeGb.Value, 1):F1} GB", VariableType.String, plugin);
        if (totalGb.HasValue)
            SetValue($"{prefix}.total", $"{(float)Math.Round(totalGb.Value, 1):F1} GB", VariableType.String, plugin);

        if (usedGb.HasValue && totalGb.HasValue && totalGb.Value > 0)
        {
            var usedPct = (float)(usedGb.Value / totalGb.Value * 100.0);
            SetValue($"{prefix}.usage", $"{usedPct:F1}%", VariableType.String, plugin);
        }

        var summary = usedGb.HasValue && totalGb.HasValue
            ? $"{usedGb.Value:F1} / {totalGb.Value:F1} GB"
            : (usedGb.HasValue ? $"{usedGb.Value:F1} GB" : "N/A");
        SetValue($"{prefix}.summary", summary, VariableType.String, plugin);
    }
}
