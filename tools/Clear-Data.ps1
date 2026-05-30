$dataPath = "D:\CodexProjects\LightStickyNote\src\LightStickyNote.App\bin\Debug\net8.0-windows\user-data"

if (Test-Path $dataPath) {
    Remove-Item -LiteralPath $dataPath -Recurse -Force
    Write-Host "Removed $dataPath"
} else {
    Write-Host "No user-data folder found at $dataPath"
}
