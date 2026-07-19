using MacroDeck.HardwareMonitor.Core.Interfaces;
using MacroDeck.HardwareMonitor.Infrastructure.Cache;
using MacroDeck.HardwareMonitor.Infrastructure.Providers;
using MacroDeck.HardwareMonitor.Infrastructure.Services;

namespace MacroDeck.HardwareMonitor.Infrastructure.Extensions;

public static class HardwareMonitorFactory
{
    public static (IHardwarePollingService PollingService, ISensorCache SensorCache) Create()
    {
        var cache = new SensorCache();
        var lhm = new LibreHardwareProvider();
        var cpu = new CpuPerformanceProvider();
        var memory = new MemoryProvider();
        var storage = new StorageProvider();
        var polling = new HardwarePollingService(lhm, cpu, memory, storage, cache);

        return (polling, cache);
    }
}
