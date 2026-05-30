. "$PSScriptRoot\Set-DevEnvironment.ps1"

& "D:\DevTools\dotnet\dotnet.exe" restore "D:\CodexProjects\LightStickyNote\LightStickyNote.sln"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

& "D:\DevTools\dotnet\dotnet.exe" build "D:\CodexProjects\LightStickyNote\LightStickyNote.sln" -c Debug
exit $LASTEXITCODE
