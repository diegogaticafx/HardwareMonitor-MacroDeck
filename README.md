# MacroDeck.HardwareMonitor

Plugin for **Macro Deck 2** that monitors hardware sensors (CPU, GPU, RAM, disk) directly from Windows, without the need for external tools running in the background.

## Features

- CPU: Usage %, Temperature
- GPU: Usage %, Temperature
- RAM: Usage %, Used/Free/Total GB
- Disk: Activity %, Used Space %, Used/Free/Total GB, 
- Polling every 1000ms (cached, no redundant API calls)
- Uses `GlobalMemoryStatusEx` (RAM), `GetSystemTimes` (CPU), `DriveInfo` + `PerformanceCounter` (Disk), LibreHardwareMonitorLib (GPU + temps)

## Variables

All variables are `String` type with the unit included.

| Variable | Example | Description |
|----------|---------|-------------|
| `cpu.usage` | 45.0% | CPU usage |
| `cpu.temp` | 65.3°C | CPU temperature |
| `gpu.usage` | 80.0% | GPU usage |
| `gpu.temp` | 72.1°C | GPU temperature |
| `ram.usage` | 65.0% | RAM usage percentage |
| `ram.used` | 8.2 GB | Used RAM |
| `ram.free` | 4.5 GB | Free RAM |
| `ram.total` | 16.0 GB | Total RAM |
| `ram.summary` | 8.2 / 16.0 GB | Used / Total summary |
| `disk.c.load` | 55.0% | Disk activity |
| `disk.c.usage` | 55.0% | Used space percentage |
| `disk.c.used` | 120.5 GB | Used space |
| `disk.c.free` | 99.5 GB | Free space |
| `disk.c.total` | 220.0 GB | Total capacity |
| `disk.c.summary` | 120.5 / 220.0 GB | Used / Total summary |

For other drives (D:, E:, etc.), replace `c` with the corresponding drive letter.

## Usage in Button Labels

Use Cottle templates:

```
CPU: {variable.cpu.usage}
Temp: {variable.cpu.temp}
GPU: {variable.gpu.usage}
RAM: {variable.ram.summary}
Disk: {variable.disk.c.summary}
Activity: {variable.disk.c.load}
```

## How to build on your own

See [BUILD.md](docs/BUILD.md).

## How to Install manually

See [INSTALL.md](docs/INSTALL.md).

## WinRing0 Driver (CPU Temperature)

Some CPUs require the WinRing0 kernel driver for temperature readings. If {cpu.temp} does not appear, install LibreHardwareMonitor standalone once to load the driver. After that, Macro Deck 2 reads CPU temperature with admin rights. 

The driver is not required for the rest of the plugin — all other variables (CPU usage, GPU usage/temp, RAM, disk) work without it.

## Architecture

See [ARCHITECTURE.md](docs/ARCHITECTURE.md).

## License

MIT
