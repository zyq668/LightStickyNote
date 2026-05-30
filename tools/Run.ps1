. "$PSScriptRoot\Set-DevEnvironment.ps1"

$exePath = "D:\CodexProjects\LightStickyNote\src\LightStickyNote.App\bin\Debug\net8.0-windows\LightStickyNote.App.exe"

if (-not (Test-Path $exePath)) {
    & "D:\DevTools\dotnet\dotnet.exe" build "D:\CodexProjects\LightStickyNote\LightStickyNote.sln" -c Debug
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Start-Process -FilePath $exePath
