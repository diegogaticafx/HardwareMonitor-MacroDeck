using MacroDeck.HardwareMonitor.Core.Models;

namespace MacroDeck.HardwareMonitor.Core.Interfaces;

public interface ILibreHardwareService : IDisposable
{
    void Open();
    void Update();
    IReadOnlyList<SensorReading> GetCpuSensors();
    IReadOnlyList<SensorReading> GetGpuSensors();
    IReadOnlyList<SensorReading> GetMemorySensors();
    IReadOnlyList<SensorReading> GetStorageSensors();
    IReadOnlyList<SensorReading> GetMotherboardSensors();
}
