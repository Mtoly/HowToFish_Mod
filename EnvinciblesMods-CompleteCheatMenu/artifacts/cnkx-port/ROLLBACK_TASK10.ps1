[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)

$ErrorActionPreference = 'Stop'
$SourceRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task10-original-src'
$resolved = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
    throw "DestinationRoot does not exist: $resolved"
}
$restore = [ordered]@{
    'WeaponCheats.cs' = 'decompiled-src\CompleteCheatMenu\Cheats\WeaponCheats.cs'
    'CheatState.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\CheatState.cs'
    'TickDriver.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\TickDriver.cs'
}
$created = @(
    'decompiled-src\CompleteCheatMenu\Targeting\VisibleAimController.cs',
    'tests\cnkx_visible_aim_impl.py',
    'tests\visible_aim_test.py'
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
$cheatHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['CheatState.cs'])).Hash
$tickHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['TickDriver.cs'])).Hash
$remaining = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $resolved $_) })
$match = $weaponHash -eq 'A7E0E2256CFEE9709246F084B129EF6C62313A545DC201E24604EB33BCABB051' -and $cheatHash -eq '9A118E2E4D033EAB6D1FD91B5C9109D7C6D77E96421AFA6F7EB1051C08E71DF9' -and $tickHash -eq 'C0E7760C49980B50EAA316DCA0BCEE88881DA47CCA7E0A6F4480E56CE7818AF4' -and $remaining.Count -eq 0
Write-Output "WEAPON_SHA256=$weaponHash"
Write-Output "CHEATSTATE_SHA256=$cheatHash"
Write-Output "TICK_DRIVER_SHA256=$tickHash"
Write-Output "CREATED_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$match"
if (-not $match) { exit 2 }
