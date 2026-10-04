[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)

$ErrorActionPreference = 'Stop'
$SourceRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task8-original-src'
$resolved = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
    throw "DestinationRoot does not exist: $resolved"
}
$restore = [ordered]@{
    'CheatState.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\CheatState.cs'
    'TargetingSystem.cs' = 'decompiled-src\CompleteCheatMenu\Targeting\TargetingSystem.cs'
    'cnkx_math_impl.py' = 'tests\cnkx_math_impl.py'
    'ballistic_prediction_test.py' = 'tests\ballistic_prediction_test.py'
}
$created = @(
    'decompiled-src\CompleteCheatMenu\Targeting\VelocityTracker.cs',
    'decompiled-src\CompleteCheatMenu\Targeting\BallisticPredictor.cs'
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
$systemHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['TargetingSystem.cs'])).Hash
$mathHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['cnkx_math_impl.py'])).Hash
$testHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['ballistic_prediction_test.py'])).Hash
$remaining = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $resolved $_) })
$match = $cheatHash -eq '9761A9E2E295B6D44BFCDF87C4D9ED6EBD1FDDF11D4C3AB142B9B1E2481840E6' -and $systemHash -eq '139C1E8A5F58B8BE4C5432BBC1359A07395059E9DCF0B30D62309DC41DDDC747' -and $mathHash -eq '5C2F5F73F5861A5C2AEA2BAD50C97C9EA101647BE8D3D0679E2CA53920D8E929' -and $testHash -eq '1C1819893C1F93891F66CA94A878367B97048D3ADF0E6345E2167C8AD928F258' -and $remaining.Count -eq 0
Write-Output "CHEATSTATE_SHA256=$cheatHash"
Write-Output "TARGETING_SYSTEM_SHA256=$systemHash"
Write-Output "MATH_IMPL_SHA256=$mathHash"
Write-Output "BALLISTIC_TEST_SHA256=$testHash"
Write-Output "CREATED_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$match"
if (-not $match) { exit 2 }
