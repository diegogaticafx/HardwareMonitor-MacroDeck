# Build Instructions

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Macro Deck 2 v2.15.0 or later

## Build

```bash
git clone https://github.com/your-username/MacroDeck.HardwareMonitor.git
cd MacroDeck.HardwareMonitor
dotnet restore
dotnet build
```

## Build Release

```bash
dotnet build -c Release
```

## Run Tests

```bash
dotnet test
```

## Publish (self-contained)

```bash
dotnet publish MacroDeck.HardwareMonitor.csproj -c Release -o publish
```
