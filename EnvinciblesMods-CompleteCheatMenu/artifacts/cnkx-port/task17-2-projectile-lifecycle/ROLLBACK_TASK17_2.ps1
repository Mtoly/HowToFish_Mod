[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)][string]$DestinationRoot,
    [string]$DestinationDll = '',
    [string]$BaselineDll = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task17-1-visibility-ui\CompleteCheatMenu.task17-1.zh-CN.dll'
)
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $root -PathType Container)) { throw "DestinationRoot does not exist: $root" }
$original = Join-Path $PSScriptRoot 'original'
$files = [ordered]@{
 'decompiled-src__CompleteCheatMenu__Plugin.cs' = 'decompiled-src\CompleteCheatMenu\Plugin.cs'
 'decompiled-src__CompleteCheatMenu__Runtime__TickDriver.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\TickDriver.cs'
 'decompiled-src__CompleteCheatMenu__UI__Tabs__DiagnosticsTab.cs' = 'decompiled-src\CompleteCheatMenu\UI\Tabs\DiagnosticsTab.cs'
 'docs__plans__2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md' = 'docs\plans\2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md'
}
foreach ($entry in $files.GetEnumerator()) {
 $destination = [IO.Path]::GetFullPath((Join-Path $root $entry.Value))
 if (-not $destination.StartsWith($root,[StringComparison]::OrdinalIgnoreCase)) { throw "Rollback destination escaped root: $destination" }
 New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
 Copy-Item -LiteralPath (Join-Path $original $entry.Key) -Destination $destination -Force
}
$created = @(
 'decompiled-src\CompleteCheatMenu\Targeting\ProjectileBindings.cs',
 'decompiled-src\CompleteCheatMenu\Targeting\ProjectileOwnership.cs',
 'decompiled-src\CompleteCheatMenu\Targeting\ProjectileTracker.cs',
 'decompiled-src\CompleteCheatMenu\Targeting\MagicShotContext.cs',
 'tests\projectile_lifecycle_test.py',
 'tests\projectile_lifecycle_harness.cs'
)
foreach ($relative in $created) {
 $path=[IO.Path]::GetFullPath((Join-Path $root $relative))
 if (-not $path.StartsWith($root,[StringComparison]::OrdinalIgnoreCase)) { throw "Rollback deletion escaped root: $path" }
 if (Test-Path -LiteralPath $path -PathType Leaf) { Remove-Item -LiteralPath $path -Force }
}
if (-not [string]::IsNullOrWhiteSpace($DestinationDll)) {
 $dll=[IO.Path]::GetFullPath($DestinationDll); New-Item -ItemType Directory -Path (Split-Path -Parent $dll) -Force | Out-Null
 Copy-Item -LiteralPath $BaselineDll -Destination $dll -Force
 Write-Output "RESTORED_DLL_SHA256=$((Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash)"
}
Write-Output 'RESTORED_SOURCE_FILES=4'
Write-Output "CREATED_FILES_PRESENT=$(@($created | Where-Object { Test-Path -LiteralPath (Join-Path $root $_) -PathType Leaf }).Count)"
Write-Output 'ROLLBACK_TASK17_2=PASS'
