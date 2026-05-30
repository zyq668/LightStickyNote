$env:DOTNET_ROOT = "D:\DevTools\dotnet"
$env:PATH = "D:\DevTools\dotnet;" + $env:PATH
$env:NUGET_PACKAGES = "D:\DevTools\nuget-packages"
$env:DOTNET_CLI_HOME = "D:\DevTools\dotnet-home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"

Write-Host "DOTNET_ROOT=$env:DOTNET_ROOT"
Write-Host "NUGET_PACKAGES=$env:NUGET_PACKAGES"
