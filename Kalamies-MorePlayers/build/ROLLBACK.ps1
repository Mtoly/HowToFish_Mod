param(
    [string] $InstalledPlugin = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\BepInEx\plugins\Kalamies-MorePlayers\MorePlayers.dll',
    [switch] $BaselineAbsent
)

$ErrorActionPreference = 'Stop'
$Original = 'D:\Code\How2fish\Kalamies-MorePlayers\MorePlayers.dll'
if ($BaselineAbsent) {
    $Directory = Split-Path -Parent $InstalledPlugin
    if (Test-Path -LiteralPath $Directory) { Remove-Item -LiteralPath $Directory -Recurse -Force }
    Write-Output "RESTORED_ABSENT=$Directory"
    exit 0
}
Copy-Item -LiteralPath $Original -Destination $InstalledPlugin -Force
$Hash = Get-FileHash -LiteralPath $InstalledPlugin -Algorithm SHA256
Write-Output "RESTORED=$InstalledPlugin"
Write-Output "SHA256=$($Hash.Hash)"
