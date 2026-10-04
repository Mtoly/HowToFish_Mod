[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)

$ErrorActionPreference = 'Stop'
$SourceRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task3-original-src'
$restore = [ordered]@{
    'AimTargetRegistry.cs' = 'decompiled-src\CompleteCheatMenu\Cheats\AimTargetRegistry.cs'
    'CreatureRegistry_Patch.cs' = 'decompiled-src\CompleteCheatMenu\Patches\CreatureRegistry_Patch.cs'
}
$created = @(
    'decompiled-src\CompleteCheatMenu\Runtime\EntityRegistry.cs',
    'decompiled-src\CompleteCheatMenu\Runtime\EntityRecord.cs',
    'decompiled-src\CompleteCheatMenu\Patches\PlayerRegistry_Patch.cs',
    'tests\entity_registry_test.py'
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
$aimHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $DestinationRoot $restore['AimTargetRegistry.cs'])).Hash
$creatureHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $DestinationRoot $restore['CreatureRegistry_Patch.cs'])).Hash
$remaining = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $DestinationRoot $_) })
$match = $aimHash -eq 'E9D43AEC1E04035C58F3E9E3F691FC7B7D00E98E0823E65D1A07BC74EAABF988' -and $creatureHash -eq 'F6F139BF865BD944C957039528617E3619B7A5E3A341D07EB67F0A5E4697ABEB' -and $remaining.Count -eq 0
Write-Output "AIM_REGISTRY_SHA256=$aimHash"
Write-Output "CREATURE_PATCH_SHA256=$creatureHash"
Write-Output "CREATED_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$match"
if (-not $match) { exit 2 }
