using MacroDeck.HardwareMonitor.Core.Models;

namespace MacroDeck.HardwareMonitor.Core.Interfaces;

public interface ISensorCache
{
    HardwareSnapshot GetSnapshot();
    void UpdateSnapshot(HardwareSnapshot snapshot);
    float? GetValue(string identifier);
    bool TryGetValue(string identifier, out float? value);
}
