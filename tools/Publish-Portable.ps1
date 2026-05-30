param(
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$projectPath = Join-Path $projectRoot "src\LightStickyNote.App\LightStickyNote.App.csproj"
$artifactsRoot = Join-Path $projectRoot "artifacts"
$portableDirectory = Join-Path $artifactsRoot "LightStickyNote-$Runtime-portable"
$zipPath = "$portableDirectory.zip"

if (Test-Path "D:\DevTools\dotnet\dotnet.exe") {
    . "$PSScriptRoot\Set-DevEnvironment.ps1"
}

if ($null -eq (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET SDK was not found. Install .NET 8 SDK before publishing."
}

$artifactsRoot = [IO.Path]::GetFullPath($artifactsRoot)
$portableDirectory = [IO.Path]::GetFullPath($portableDirectory)
$zipPath = [IO.Path]::GetFullPath($zipPath)

if (-not $portableDirectory.StartsWith(
    $artifactsRoot + [IO.Path]::DirectorySeparatorChar,
    [StringComparison]::OrdinalIgnoreCase)) {
    throw "Portable output must stay inside the artifacts directory."
}

if (Test-Path $portableDirectory) {
    Remove-Item -LiteralPath $portableDirectory -Recurse -Force
}

if (Test-Path $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

New-Item -ItemType Directory -Force -Path $portableDirectory | Out-Null

& dotnet publish $projectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -o $portableDirectory

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

$portableReadme =
@"
LightStickyNote Portable

1. Extract this archive to a writable folder.
2. Double-click LightStickyNote.App.exe to start.
3. The app creates a user-data folder beside the EXE for SQLite data and settings.
4. Keep the EXE and user-data folder together when moving the app.
"@

Set-Content -LiteralPath (Join-Path $portableDirectory "README-portable.txt") `
    -Value $portableReadme `
    -Encoding UTF8

Compress-Archive -Path (Join-Path $portableDirectory "*") -DestinationPath $zipPath

Write-Host "Portable directory: $portableDirectory"
Write-Host "Portable archive:   $zipPath"
