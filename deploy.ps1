param(
    [string]$Configuration = "Debug",
    [string]$PluginDir = "$env:APPDATA\Macro Deck\plugins\MacroDeck.HardwareMonitor"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$publishDir = Join-Path $root "publish-output"

Write-Host "=== MacroDeck.HardwareMonitor - Deploy ===" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration"
Write-Host "Plugin Dir: $PluginDir"
Write-Host ""

# 1. Build
Write-Host "[1/4] Building..." -ForegroundColor Yellow
dotnet build $root\MacroDeck.HardwareMonitor.slnx -c $Configuration
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

# 2. Publish (resolves all NuGet dependencies transitively)
Write-Host "[2/4] Publishing dependencies..." -ForegroundColor Yellow
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }
dotnet publish $root\src\MacroDeck.HardwareMonitor\MacroDeck.HardwareMonitor.csproj -c $Configuration -o $publishDir --no-build
if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

# 3. Create plugin directory and copy files
Write-Host "[3/4] Copying files..." -ForegroundColor Yellow
New-Item -ItemType Directory -Path $PluginDir -Force | Out-Null

# Cleanup old artifacts
Remove-Item "$PluginDir\Plugin.xml" -Force -ErrorAction SilentlyContinue

# Copy all DLLs from publish output (skip assemblies owned by the .NET host)
Get-ChildItem "$publishDir\*.dll" | ForEach-Object {
    $skip = @(
        "WinRT.Runtime.dll",
        "Microsoft.Windows.SDK.NET.dll"
    )
    if ($_.Name -notin $skip) {
        Copy-Item $_.FullName "$PluginDir\" -Force
        Write-Host "  $($_.Name)" -ForegroundColor Gray
    }
}

# Ensure LibreHardwareMonitorLib.dll is present (publish may skip it)
$lhmNugetPath = "$env:USERPROFILE\.nuget\packages\librehardwaremonitorlib\0.9.6"
$lhmSource = "$lhmNugetPath\runtimes\win-x64\lib\net10.0\LibreHardwareMonitorLib.dll"
if (Test-Path $lhmSource) {
    Copy-Item $lhmSource "$PluginDir\LibreHardwareMonitorLib.dll" -Force
    Write-Host "  LibreHardwareMonitorLib.dll" -ForegroundColor Gray
}

# ExtensionManifest.json
Copy-Item "$root\ExtensionManifest.json" "$PluginDir\" -Force

# Extension icon (MacroDeck convention: ExtensionIcon.png)
if (Test-Path "$root\ExtensionIcon.png") {
    Copy-Item "$root\ExtensionIcon.png" "$PluginDir\" -Force
    Write-Host "  ExtensionIcon.png" -ForegroundColor Gray
}

# Cleanup
Remove-Item $publishDir -Recurse -Force

# 4. Verify
Write-Host "[4/4] Verifying..." -ForegroundColor Yellow
$files = Get-ChildItem $PluginDir
Write-Host ""
Write-Host "=== Files in $PluginDir ===" -ForegroundColor Cyan
$files | ForEach-Object { Write-Host "  $($_.Name) ($( [math]::Round($_.Length / 1KB, 1) ) KB)" }

Write-Host ""
Write-Host "Deploy complete! Restart Macro Deck 2." -ForegroundColor Green
