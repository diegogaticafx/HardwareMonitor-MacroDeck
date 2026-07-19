# Architecture

## Overview

```
┌─────────────────────────────────────────────────┐
│              Macro Deck 2 Host                   │
│  ┌───────────────────────────────────────────┐  │
│  │        MacroDeck.HardwareMonitor          │  │
│  │  ┌─────────────────────────────────────┐  │  │
│  │  │              Main.cs                │  │  │
│  │  │             (Plugin)                │  │  │
│  │  │                                     │  │  │
│  │  │          Variable                   │  │  │
│  │  │          Registrar                  │  │  │
│  │  └─────────────────┬───────────────────┘  │  │
│  └───────┼───────────────────────────────────┘  │
└──────────┼────────────────────────────────────┘
           │
┌──────────▼────────────────────────────────────┐
│         Core (Pure Logic)                     │
│  ┌──────────────┐  ┌────────────────────────┐│
│  │   Models     │  │    Interfaces          ││
│  │ - SensorType │  │ - IHardwarePollingSvc  ││
│  │ - SensorRead │  │ - ISensorCache         ││
│  │ - HardwareSp │  │ - ILibreHardwareSvc    ││
│  └──────────────┘  └────────────────────────┘│
└───────────────────────────────────────────────┘
           │
┌──────────▼────────────────────────────────────┐
│     Infrastructure (Implementation)           │
│  ┌────────────┐ ┌──────────┐ ┌─────────────┐ │
│  │ LHM        │ │ CPU Perf │ │ Memory      │ │
│  │ Provider   │ │ Counter  │ │ provider    │ │
│  ├────────────┤ ├──────────┤ ├─────────────┤ │
│  │ Storage    │ │ Sensor   │ │ Polling     │ │
│  │ Provider   │ │ Cache    │ │ Service     │ │
│  └────────────┘ └──────────┘ └─────────────┘ │
└───────────────────────────────────────────────┘
```

## Project Structure

### MacroDeck.HardwareMonitor.Core
Pure business logic with no external dependencies.

- **Models**: `SensorType`, `SensorReading`, `HardwareSnapshot`
- **Interfaces**: `ILibreHardwareService`, `ISensorCache`, `IHardwarePollingService`

### MacroDeck.HardwareMonitor.Infrastructure
All external access implementations.

- **Providers**: Wrappers for LibreHardwareMonitorLib, P/Invoke calls for CPU/RAM
- **Cache**: Thread-safe sensor cache using `ConcurrentDictionary`
- **Services**: Polling service with `System.Timers.Timer`

### MacroDeck.HardwareMonitor
Macro Deck 2 plugin entry point.

- **Main.cs**: Plugin lifecycle, DI wiring
- **VariableRegistrar**: Updates Macro Deck variables from sensor cache

## Data Flow

1. `HardwarePollingService` fires every 1000ms
2. Collects data from LibreHardwareMonitor + Windows APIs
3. Builds a `HardwareSnapshot` (immutable)
4. Updates `SensorCache`
5. Fires `OnPollingCompleted` event
6. `VariableRegistrar.UpdateAll()` reads cache and updates `VariableManager`
7. Macro Deck UI renders buttons using Cottle templates

## Key Design Decisions

- **No DI container**: Macro Deck plugins are loaded as raw DLLs. Dependencies are manually wired in `Main.Enable()`.
- **No BackgroundService**: Uses `System.Timers.Timer` which aligns with Macro Deck's threading model.
- **Thread safety**: All LHM access is locked. Sensor cache uses `ConcurrentDictionary` internally.
- **Performance counters avoided**: CPU usage via `GetSystemTimes` P/Invoke, RAM via `GlobalMemoryStatusEx` — both faster than WMI.
