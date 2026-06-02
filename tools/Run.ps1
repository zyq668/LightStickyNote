. "$PSScriptRoot\Set-DevEnvironment.ps1"

$repoRoot = Split-Path -Path $PSScriptRoot -Parent
$solutionPath = Join-Path $repoRoot "LightStickyNote.sln"
$appPath = Join-Path $repoRoot "src\LightStickyNote.App\bin\Debug\net8.0-windows\LightStickyNote.App.dll"
$dotnetPath = "D:\DevTools\dotnet\dotnet.exe"

$runningApp = Get-CimInstance Win32_Process |
    Where-Object { $_.Name -eq "dotnet.exe" -and $_.CommandLine -match "LightStickyNote\.App\.dll" } |
    Select-Object -First 1

if ($runningApp) {
    Write-Host ""
    Write-Host "Light Sticky Note is already running. Exit it from the tray before reopening the latest version." -ForegroundColor Yellow
    Read-Host "Press Enter to close"
    exit 0
}

& $dotnetPath build $solutionPath -c Debug
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Build failed. If Light Sticky Note is already running, exit it from the tray and try again." -ForegroundColor Yellow
    Read-Host "Press Enter to close"
    exit $LASTEXITCODE
}

Start-Process -FilePath $dotnetPath -ArgumentList $appPath
