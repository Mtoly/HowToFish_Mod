[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$DestinationRoot,
    [string]$DestinationDll = '',
    [string]$BaselineDll = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\CompleteCheatMenu.client-mode.zh-CN.dll'
)

$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $root -PathType Container)) {
    throw "DestinationRoot does not exist: $root"
}

$original = Join-Path $PSScriptRoot 'original'
$files = [ordered]@{
    'decompiled-src__CompleteCheatMenu__Targeting__TargetingSystem.cs' = 'decompiled-src\CompleteCheatMenu\Targeting\TargetingSystem.cs'
    'decompiled-src__CompleteCheatMenu__Targeting__VisibilityCache.cs' = 'decompiled-src\CompleteCheatMenu\Targeting\VisibilityCache.cs'
    'decompiled-src__CompleteCheatMenu__Runtime__EspRendererV2.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\EspRendererV2.cs'
    'decompiled-src__CompleteCheatMenu__UI__Tabs__WeaponsTab.cs' = 'decompiled-src\CompleteCheatMenu\UI\Tabs\WeaponsTab.cs'
    'tests__esp_projection_test.py' = 'tests\esp_projection_test.py'
    'docs__plans__2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md' = 'docs\plans\2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md'
}

$restored = 0
foreach ($entry in $files.GetEnumerator()) {
    $source = Join-Path $original $entry.Key
    $destination = [IO.Path]::GetFullPath((Join-Path $root $entry.Value))
    if (-not $destination.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Rollback destination escaped root: $destination"
    }
    New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
    Copy-Item -LiteralPath $source -Destination $destination -Force
    $restored++
}

$createdTest = [IO.Path]::GetFullPath((Join-Path $root 'tests\visibility_ui_semantics_test.py'))
if ($createdTest.StartsWith($root, [StringComparison]::OrdinalIgnoreCase) -and (Test-Path -LiteralPath $createdTest -PathType Leaf)) {
    Remove-Item -LiteralPath $createdTest -Force
}

if (-not [string]::IsNullOrWhiteSpace($DestinationDll)) {
    $dll = [IO.Path]::GetFullPath($DestinationDll)
    New-Item -ItemType Directory -Path (Split-Path -Parent $dll) -Force | Out-Null
    Copy-Item -LiteralPath $BaselineDll -Destination $dll -Force
    Write-Output "RESTORED_DLL_SHA256=$((Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash)"
}

Write-Output "RESTORED_SOURCE_FILES=$restored"
Write-Output "CREATED_TEST_PRESENT=$([bool](Test-Path -LiteralPath $createdTest -PathType Leaf))"
Write-Output 'ROLLBACK_TASK17_1=PASS'
