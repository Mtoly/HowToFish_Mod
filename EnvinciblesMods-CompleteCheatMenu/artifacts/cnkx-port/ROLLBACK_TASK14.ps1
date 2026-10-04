[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)
$ErrorActionPreference = 'Stop'
$SourceRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task14-original-src'
$resolved = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $resolved -PathType Container)) { throw "DestinationRoot does not exist: $resolved" }
$plan = 'docs\plans\2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md'
$planDestination = Join-Path $resolved $plan
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $planDestination) | Out-Null
Copy-Item -LiteralPath (Join-Path $SourceRoot '2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md') -Destination $planDestination -Force
$dllDestination = Join-Path $resolved 'artifacts\CompleteCheatMenu.dll'
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dllDestination) | Out-Null
Copy-Item -LiteralPath (Join-Path $SourceRoot 'CompleteCheatMenu.baseline.dll') -Destination $dllDestination -Force
$created = @(
    'tests\cnkx_performance_harness.cs',
    'artifacts\cnkx-port\performance.txt',
    'artifacts\cnkx-port\review.md',
    'artifacts\cnkx-port\cnkx_performance_harness.exe'
)
foreach ($relative in $created) { $target = Join-Path $resolved $relative; if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Force } }
$planHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $planDestination).Hash
$dllHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $dllDestination).Hash
$remaining = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $resolved $_) })
$match = $dllHash -eq 'AB57B01D8326EC3472CA8AC306EB1D434A4B046A819EFC07D456DC36A2498391'
Write-Output "PLAN_SHA256=$planHash"
Write-Output "DLL_SHA256=$dllHash"
Write-Output "CREATED_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$match"
if (-not $match -or $remaining.Count -ne 0) { exit 2 }
exit 0
