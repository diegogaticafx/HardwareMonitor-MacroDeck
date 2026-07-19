using System.Collections.Concurrent;
using MacroDeck.HardwareMonitor.Core.Interfaces;
using MacroDeck.HardwareMonitor.Core.Models;

namespace MacroDeck.HardwareMonitor.Infrastructure.Cache;

public sealed class SensorCache : ISensorCache
{
    private HardwareSnapshot _snapshot = HardwareSnapshot.Empty;
    private readonly ConcurrentDictionary<string, float?> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    public HardwareSnapshot GetSnapshot()
    {
        lock (_lock) { return _snapshot; }
    }

    public void UpdateSnapshot(HardwareSnapshot snapshot)
    {
        lock (_lock)
        {
            _snapshot = snapshot;
            _values.Clear();

            AddToCache(snapshot.CpuSensors);
            AddToCache(snapshot.GpuSensors);
            AddToCache(snapshot.MemorySensors);
            AddToCache(snapshot.StorageSensors);
        }
    }

    public float? GetValue(string identifier)
    {
        _values.TryGetValue(identifier, out var value);
        return value;
    }

    public bool TryGetValue(string identifier, out float? value)
    {
        return _values.TryGetValue(identifier, out value);
    }

    private void AddToCache(IReadOnlyList<SensorReading> sensors)
    {
        for (int i = 0; i < sensors.Count; i++)
        {
            _values[sensors[i].Identifier] = sensors[i].Value;
        }
    }
}
