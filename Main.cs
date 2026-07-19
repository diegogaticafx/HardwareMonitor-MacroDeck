using MacroDeck.HardwareMonitor.Core.Interfaces;
using MacroDeck.HardwareMonitor.Infrastructure.Extensions;
using SuchByte.MacroDeck.Plugins;

namespace MacroDeck.HardwareMonitor;

public class Main : MacroDeckPlugin
{
    internal static Main? Instance { get; private set; }
    internal ISensorCache SensorCache { get; private set; } = null!;

    private IHardwarePollingService _pollingService = null!;
    private bool _initialized;

    public Main()
    {
        Instance = this;

        if (_initialized) return;

        try
        {
            var factory = HardwareMonitorFactory.Create();
            _pollingService = factory.PollingService;
            SensorCache = factory.SensorCache;

            _pollingService.OnPollingCompleted += OnPollingCompleted;
            _pollingService.Start(1000);

            _initialized = true;
        }
        catch
        {
            throw;
        }
    }

    public override void Enable()
    {
        if (_initialized) return;

        try
        {
            var factory = HardwareMonitorFactory.Create();
            _pollingService = factory.PollingService;
            SensorCache = factory.SensorCache;

            _pollingService.OnPollingCompleted += OnPollingCompleted;
            _pollingService.Start(1000);

            _initialized = true;
        }
        catch { }
    }

    public void Dispose()
    {
        if (!_initialized) return;
        _pollingService.OnPollingCompleted -= OnPollingCompleted;
        _pollingService.Dispose();
        _initialized = false;
        Instance = null;
    }

    private void OnPollingCompleted(object? sender, EventArgs e)
    {
        var snapshot = SensorCache.GetSnapshot();
        VariableRegistrar.UpdateAll(snapshot, this);
    }
}
