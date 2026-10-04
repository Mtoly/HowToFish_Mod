[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)

$ErrorActionPreference = 'Stop'
$SourceRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task11-original-src'
$resolved = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
    throw "DestinationRoot does not exist: $resolved"
}
$restore = [ordered]@{
    'CheatState.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\CheatState.cs'
    'Plugin.cs' = 'decompiled-src\CompleteCheatMenu\Plugin.cs'
    'WeaponCheats.cs' = 'decompiled-src\CompleteCheatMenu\Cheats\WeaponCheats.cs'
}
$created = @(
    'decompiled-src\CompleteCheatMenu\Cheats\WeaponStateStore.cs',
    'tests\cnkx_weapon_store_impl.py',
    'tests\weapon_restore_test.py'
)
foreach ($entry in $restore.GetEnumerator()) {
    $destination = Join-Path $resolved $entry.Value
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
    Copy-Item -LiteralPath (Join-Path $SourceRoot $entry.Key) -Destination $destination -Force
}
foreach ($relative in $created) {
    $target = Join-Path $resolved $relative
    if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Force }
}
$cheatHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['CheatState.cs'])).Hash
$pluginHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['Plugin.cs'])).Hash
$weaponHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['WeaponCheats.cs'])).Hash
$remaining = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $resolved $_) })
$match = $cheatHash -eq '11BC1B747042BD019FC1B5126084FFB5D00CEE4770C093BF21E4FE97176CB5D5' -and $pluginHash -eq '86A84F114A2E0AC657F0B53D9354C97D6652F9D6310061F0EEEAD9705BD8FBFC' -and $weaponHash -eq 'C5E2A322C2B0BF65A57E52C44F8D09A574936DF026D78480858CDC4DB868732F' -and $remaining.Count -eq 0
Write-Output "CHEATSTATE_SHA256=$cheatHash"
Write-Output "PLUGIN_SHA256=$pluginHash"
Write-Output "WEAPON_SHA256=$weaponHash"
Write-Output "CREATED_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$match"
if (-not $match) { exit 2 }
