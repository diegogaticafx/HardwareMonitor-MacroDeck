using LibreHardwareMonitor.Hardware;
using MacroDeck.HardwareMonitor.Core.Interfaces;
using MacroDeck.HardwareMonitor.Core.Models;
using LhmSensorType = LibreHardwareMonitor.Hardware.SensorType;

namespace MacroDeck.HardwareMonitor.Infrastructure.Providers;

public sealed class LibreHardwareProvider : ILibreHardwareService
{
    private Computer? _computer;
    private readonly object _lock = new();

    public void Open()
    {
        lock (_lock)
        {
            if (_computer is not null) return;

            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsStorageEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true
            };

            _computer.Open();
        }
    }

    public void Update()
    {
        lock (_lock)
        {
            _computer?.Accept(new UpdateVisitor());
        }
    }

    public IReadOnlyList<SensorReading> GetCpuSensors()
    {
        var result = new List<SensorReading>();
        lock (_lock)
        {
            if (_computer is null) return result;

            foreach (var hw in _computer.Hardware)
            {
                if (hw.HardwareType is HardwareType.Cpu or HardwareType.SuperIO)
                {
                    AddSensors(hw, result, "cpu");
                    foreach (var sub in hw.SubHardware)
                    {
                        AddSensors(sub, result, "cpu_core");
                    }
                }
            }
        }
        return result;
    }

    public IReadOnlyList<SensorReading> GetGpuSensors()
    {
        var result = new List<SensorReading>();
        lock (_lock)
        {
            if (_computer is null) return result;

            foreach (var hw in _computer.Hardware)
            {
                if (hw.HardwareType is HardwareType.GpuAmd or HardwareType.GpuNvidia or HardwareType.GpuIntel)
                {
                    AddSensors(hw, result, "gpu");
                }
            }
        }
        return result;
    }

    public IReadOnlyList<SensorReading> GetMemorySensors()
    {
        var result = new List<SensorReading>();
        lock (_lock)
        {
            if (_computer is null) return result;

            foreach (var hw in _computer.Hardware)
            {
                if (hw.HardwareType == HardwareType.Memory)
                {
                    AddSensors(hw, result, "ram");
                }
            }
        }
        return result;
    }

    public IReadOnlyList<SensorReading> GetStorageSensors()
    {
        var result = new List<SensorReading>();
        lock (_lock)
        {
            if (_computer is null) return result;

            foreach (var hw in _computer.Hardware)
            {
                if (hw.HardwareType == HardwareType.Storage)
                {
                    var driveLetter = ExtractDriveLetter(hw.Name);
                    var prefix = string.IsNullOrEmpty(driveLetter) ? "disk" : $"disk.{driveLetter}";
                    AddSensors(hw, result, prefix);
                    foreach (var sub in hw.SubHardware)
                    {
                        AddSensors(sub, result, prefix);
                    }
                }
            }
        }
        return result;
    }

    public IReadOnlyList<SensorReading> GetMotherboardSensors()
    {
        var result = new List<SensorReading>();
        lock (_lock)
        {
            if (_computer is null) return result;

            foreach (var hw in _computer.Hardware)
            {
                if (hw.HardwareType is HardwareType.Motherboard or HardwareType.SuperIO)
                {
                    AddSensors(hw, result, "mb");
                }
            }
        }
        return result;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_computer is not null)
            {
                _computer.Close();
                _computer = null;
            }
        }
    }

    private static void AddSensors(IHardware hardware, List<SensorReading> readings, string prefix)
    {
        foreach (var sensor in hardware.Sensors)
        {
            var identifier = $"{prefix}.{sensor.SensorType.ToString().ToLowerInvariant()}.{SanitizeName(sensor.Name)}";
            var type = MapSensorType(sensor.SensorType);
            readings.Add(new SensorReading(
                identifier,
                sensor.Name,
                type,
                sensor.Value,
                GetUnit(type),
                DateTime.UtcNow));
        }
    }

    private static Core.Models.SensorType MapSensorType(LhmSensorType type)
    {
        return type switch
        {
            LhmSensorType.Temperature => Core.Models.SensorType.Temperature,
            LhmSensorType.Load => Core.Models.SensorType.Load,
            LhmSensorType.Clock => Core.Models.SensorType.Clock,
            LhmSensorType.Voltage => Core.Models.SensorType.Voltage,
            LhmSensorType.Power => Core.Models.SensorType.Power,
            LhmSensorType.Fan => Core.Models.SensorType.Fan,
            LhmSensorType.Data => Core.Models.SensorType.Data,
            _ => Core.Models.SensorType.Data
        };
    }

    private static string GetUnit(Core.Models.SensorType type)
    {
        return type switch
        {
            Core.Models.SensorType.Temperature => "°C",
            Core.Models.SensorType.Load => "%",
            Core.Models.SensorType.Clock => "MHz",
            Core.Models.SensorType.Voltage => "V",
            Core.Models.SensorType.Power => "W",
            Core.Models.SensorType.Fan => "RPM",
            Core.Models.SensorType.Data => "",
            _ => ""
        };
    }

    private static string SanitizeName(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "_")
            .Replace("#", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("/", "_")
            .Replace("-", "_");
    }

    private static string ExtractDriveLetter(string name)
    {
        var parts = name.Split(' ');
        foreach (var part in parts)
        {
            if (part.Length == 2 && part[1] == ':')
                return part[0].ToString().ToLowerInvariant();
        }
        return string.Empty;
    }

    private sealed class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer) => computer.Traverse(this);
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (var sub in hardware.SubHardware)
                sub.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}
