[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)

$ErrorActionPreference = 'Stop'
$SourceRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task7-original-src'
$resolved = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
    throw "DestinationRoot does not exist: $resolved"
}
$restore = [ordered]@{
    'WeaponCheats.cs' = 'decompiled-src\CompleteCheatMenu\Cheats\WeaponCheats.cs'
    'WeaponsTab.cs' = 'decompiled-src\CompleteCheatMenu\UI\Tabs\WeaponsTab.cs'
    'snapshot_isolation_test.py' = 'tests\snapshot_isolation_test.py'
}
$created = @(
    'decompiled-src\CompleteCheatMenu\Targeting\VisibilityCache.cs',
    'decompiled-src\CompleteCheatMenu\Targeting\TargetingSystem.cs',
    'tests\cnkx_target_lock_impl.py',
    'tests\target_lock_test.py'
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
$weaponHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['WeaponCheats.cs'])).Hash
$tabHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['WeaponsTab.cs'])).Hash
$snapshotHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['snapshot_isolation_test.py'])).Hash
$remaining = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $resolved $_) })
$match = $weaponHash -eq '77F5BC19FE6D7F5ECC0E37F56728FCA811C58A98BAE675282C3F63798527274D' -and $tabHash -eq '3AC916ED304208B69C3CEEE0927E38779C04787BE2DFAB926249D609E7991271' -and $snapshotHash -eq '6916348C59CEAEE39AF143EF3FC113493FFA9EA6DFDA8C40736352054978201F' -and $remaining.Count -eq 0
Write-Output "WEAPON_SHA256=$weaponHash"
Write-Output "WEAPONS_TAB_SHA256=$tabHash"
Write-Output "SNAPSHOT_TEST_SHA256=$snapshotHash"
Write-Output "CREATED_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$match"
if (-not $match) { exit 2 }
