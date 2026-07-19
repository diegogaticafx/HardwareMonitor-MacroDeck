# Installation Instructions

## Method 1: Via Extension Store (future)

1. Open Macro Deck 2
2. Go to Package Manager > Extensions Store
3. Search for "HardwareMonitor"
4. Click Install

## Method 2: Manual Installation

1. Build the plugin (see [BUILD.md](BUILD.md))
2. Run the deploy script:
   ```powershell
   .\deploy.ps1 -Configuration Release
   ```
   This copies all required files to `%AppData%\Macro Deck\plugins\MacroDeck.HardwareMonitor\`
3. Restart Macro Deck 2
4. The variables will appear automatically in the Variable Manager

## Requirements

- Windows 10/11 (x64)
- Macro Deck 2 v2.15.0+
- .NET 10 Runtime (included with Macro Deck)
- Administrator rights are **not required** for the plugin itself

### WinRing0 Driver (CPU Temperature)

Some CPUs require the **WinRing0** kernel driver for temperature readings. If `{cpu.temp}` does not appear, install any hardware monitoring tool once:

- **LibreHardwareMonitor** (recommended) — [librehardwaremonitor.org](https://librehardwaremonitor.org)

Admin rights are needed only for that first install. After that, Macro Deck 2 reads CPU temperature normally with admin rights.

The driver is **not required** for the rest of the plugin — all other variables work without it.
