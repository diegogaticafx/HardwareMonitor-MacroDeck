using MacroDeck.HardwareMonitor.Core.Interfaces;
using MacroDeck.HardwareMonitor.Core.Models;
using MacroDeck.HardwareMonitor.Infrastructure.Providers;
using Timer = System.Timers.Timer;

namespace MacroDeck.HardwareMonitor.Infrastructure.Services;

public sealed class HardwarePollingService : IHardwarePollingService
{
    private readonly ILibreHardwareService _lhm;
    private readonly CpuPerformanceProvider _cpu;
    private readonly MemoryProvider _memory;
    private readonly StorageProvider _storage;
    private readonly ISensorCache _cache;

    private Timer? _timer;
    private bool _disposed;

    public event EventHandler? OnPollingCompleted;

    public bool IsRunning => _timer?.Enabled ?? false;

    public HardwarePollingService(
        ILibreHardwareService lhm,
        CpuPerformanceProvider cpu,
        MemoryProvider memory,
        StorageProvider storage,
        ISensorCache cache)
    {
        _lhm = lhm ?? throw new ArgumentNullException(nameof(lhm));
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public void Start(int intervalMs = 1000)
    {
        ThrowIfDisposed();

        if (_timer is not null)
            Stop();

        _lhm.Open();

        _timer = new Timer(intervalMs);
        _timer.Elapsed += async (_, _) => await PollAsync();
        _timer.AutoReset = false;
        _timer.Start();
    }

    public void Stop()
    {
        if (_timer is not null)
        {
            _timer.Stop();
            _timer.Dispose();
            _timer = null;
        }
    }

    private async Task PollAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                _lhm.Update();

                var lhmCpu = _lhm.GetCpuSensors();
                var lhmGpu = _lhm.GetGpuSensors();
                var lhmMemory = _lhm.GetMemorySensors();
                var lhmStorage = _lhm.GetStorageSensors();

                var cpuUsage = _cpu.GetUsage();
                var memoryReadings = _memory.GetReadings();
                var storageReadings = _storage.GetReadings();

                var allCpu = new List<SensorReading>(lhmCpu.Count + 1);
                allCpu.Add(cpuUsage);
                allCpu.AddRange(lhmCpu);

                var allMemory = new List<SensorReading>(lhmMemory.Count + memoryReadings.Count);
                allMemory.AddRange(memoryReadings);
                allMemory.AddRange(lhmMemory);

                var allStorage = new List<SensorReading>(lhmStorage.Count + storageReadings.Count);
                allStorage.AddRange(storageReadings);
                allStorage.AddRange(lhmStorage);

                var snapshot = new HardwareSnapshot(
                    allCpu.AsReadOnly(),
                    lhmGpu,
                    allMemory.AsReadOnly(),
                    allStorage.AsReadOnly(),
                    DateTime.UtcNow);

                _cache.UpdateSnapshot(snapshot);

                OnPollingCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
            }
        });

        if (_timer is not null && !_disposed)
        {
            try { _timer.Start(); } catch { }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Stop();
        _lhm.Dispose();
        _cpu.Dispose();
        _storage.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HardwarePollingService));
    }
}
