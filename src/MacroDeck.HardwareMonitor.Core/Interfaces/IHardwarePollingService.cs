namespace MacroDeck.HardwareMonitor.Core.Interfaces;

public interface IHardwarePollingService : IDisposable
{
    event EventHandler? OnPollingCompleted;
    void Start(int intervalMs = 1000);
    void Stop();
    bool IsRunning { get; }
}
