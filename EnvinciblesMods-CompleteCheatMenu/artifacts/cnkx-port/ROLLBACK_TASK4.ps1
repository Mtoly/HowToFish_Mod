[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)

$ErrorActionPreference = 'Stop'
$SourceRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task4-original-src'
$restore = [ordered]@{
    'VisualCheats.cs' = 'decompiled-src\CompleteCheatMenu\Cheats\VisualCheats.cs'
    'TickDriver.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\TickDriver.cs'
    'WeaponCheats.cs' = 'decompiled-src\CompleteCheatMenu\Cheats\WeaponCheats.cs'
}
$created = @(
    'decompiled-src\CompleteCheatMenu\Runtime\EspSnapshot.cs',
    'decompiled-src\CompleteCheatMenu\Runtime\EspSnapshotBuilder.cs',
    'decompiled-src\CompleteCheatMenu\Targeting\TargetingSnapshot.cs',
    'tests\snapshot_isolation_test.py'
)
foreach ($entry in $restore.GetEnumerator()) {
    $destination = Join-Path $DestinationRoot $entry.Value
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
    Copy-Item -LiteralPath (Join-Path $SourceRoot $entry.Key) -Destination $destination -Force
}
foreach ($relative in $created) {
    $target = Join-Path $DestinationRoot $relative
    if (Test-Path -LiteralPath $target) {
        Remove-Item -LiteralPath $target -Force
    }
}
$visualHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $DestinationRoot $restore['VisualCheats.cs'])).Hash
$tickHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $DestinationRoot $restore['TickDriver.cs'])).Hash
$weaponHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $DestinationRoot $restore['WeaponCheats.cs'])).Hash
$remaining = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $DestinationRoot $_) })
$match = $visualHash -eq '8EB9250EC1A39203F45078A8BAF743A59647C3AE35828B5E38DB971118DD3556' -and $tickHash -eq '2AA41CC3F5DE2C4AA6111EEFABEB6D0A7E980362B2B04CA50E278B4D305C394C' -and $weaponHash -eq 'D9FF172F97F8A4CAFB4EA2FFF7E2E36033191994028DFA83B61DAD49440D3916' -and $remaining.Count -eq 0
Write-Output "VISUAL_SHA256=$visualHash"
Write-Output "TICK_SHA256=$tickHash"
Write-Output "WEAPON_SHA256=$weaponHash"
Write-Output "CREATED_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$match"
if (-not $match) { exit 2 }
