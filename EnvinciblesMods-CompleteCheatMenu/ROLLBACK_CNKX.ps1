[CmdletBinding()]
param(
    [string]$DestinationDll = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\BepInEx\plugins\EnvinciblesMods-CompleteCheatMenu\CompleteCheatMenu.dll'
)
$ErrorActionPreference = 'Stop'
$baseline = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\CompleteCheatMenu.pre-cnkx.dll'
$expected = 'F4B5209647A60C0BE7E5C49005867A4CE094FEB56C03AD804AEC67C35FE3BA79'
if (-not (Test-Path -LiteralPath $baseline -PathType Leaf)) { throw "Baseline missing: $baseline" }
$resolved = [IO.Path]::GetFullPath($DestinationDll)
New-Item -ItemType Directory -Path (Split-Path -Parent $resolved) -Force | Out-Null
Copy-Item -LiteralPath $baseline -Destination $resolved -Force
$actual = (Get-FileHash -LiteralPath $resolved -Algorithm SHA256).Hash
Write-Output "RESTORED_DLL=$resolved"
Write-Output "RESTORED_SHA256=$actual"
Write-Output "ROLLBACK_MATCH=$($actual -eq $expected)"
if ($actual -ne $expected) { exit 2 }
exit 0
