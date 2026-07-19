# Architecture

## Overview

```
┌─────────────────────────────────────────────────┐
│              Macro Deck 2 Host                   │
│  ┌───────────────────────────────────────────┐  │
│  │        MacroDeck.HardwareMonitor          │  │
│  │  ┌─────────────────────────────────────┐  │  │
│  │  │    MacroDeck.HardwareMonitor.dll    │  │  │
│  │  │  ┌──────────────┐ ┌──────────────┐ │  │  │
│  │  │  │   Main.cs    │ │   Variable   │ │  │  │
│  │  │  │  (Plugin)    │ │  Registrar   │ │  │  │
│  │  │  └──────┬───────┘ └──────────────┘ │  │  │
│  │  │         │                          │  │  │
│  │  │  ┌──────▼───────────────────────┐  │  │  │
│  │  │  │   Core/ (Models + Interfaces)│  │  │  │
│  │  │  │   Infrastructure/ (Providers │  │  │  │
│  │  │  │   Cache, Services, Factory)  │  │  │  │
│  │  │  └──────────────────────────────┘  │  │  │
│  │  └─────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

## Project Structure

All code is in a single project `MacroDeck.HardwareMonitor.csproj`, organized in folders:

### `Core/` — Models and Interfaces
Pure logic with no external dependencies.

- **Interfaces**: `ILibreHardwareService`, `ISensorCache`, `IHardwarePollingService`
- **Models**: `SensorType`, `SensorReading`, `HardwareSnapshot`

### `Infrastructure/` — Implementations
All external access implementations.

- **Providers**: Wrappers for LibreHardwareMonitorLib, P/Invoke calls for CPU/RAM
- **Cache**: Thread-safe sensor cache using `ConcurrentDictionary`
- **Services**: Polling service with `System.Timers.Timer`

### Root files — Plugin entry point

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
