[CmdletBinding()]
param(
 [Parameter(Mandatory=$true)][string]$DestinationRoot,
 [string]$DestinationDll='',
 [string]$BaselineDll='D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task17-2-projectile-lifecycle\CompleteCheatMenu.task17-2.zh-CN.dll'
)
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath($DestinationRoot)
if(-not(Test-Path -LiteralPath $root -PathType Container)){throw "DestinationRoot does not exist: $root"}
$original=Join-Path $PSScriptRoot 'original'
$files=[ordered]@{
 'decompiled-src__CompleteCheatMenu__Patches__WeaponShoot_Patch.cs'='decompiled-src\CompleteCheatMenu\Patches\WeaponShoot_Patch.cs'
 'decompiled-src__CompleteCheatMenu__Cheats__WeaponCheats.cs'='decompiled-src\CompleteCheatMenu\Cheats\WeaponCheats.cs'
 'decompiled-src__CompleteCheatMenu__Targeting__ProjectileBindings.cs'='decompiled-src\CompleteCheatMenu\Targeting\ProjectileBindings.cs'
 'decompiled-src__CompleteCheatMenu__Targeting__MagicShotContext.cs'='decompiled-src\CompleteCheatMenu\Targeting\MagicShotContext.cs'
 'tests__tracking_probability_test.py'='tests\tracking_probability_test.py'
 'tests__projectile_lifecycle_harness.cs'='tests\projectile_lifecycle_harness.cs'
 'docs__plans__2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md'='docs\plans\2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md'
}
foreach($entry in $files.GetEnumerator()){
 $destination=[IO.Path]::GetFullPath((Join-Path $root $entry.Value))
 if(-not $destination.StartsWith($root,[StringComparison]::OrdinalIgnoreCase)){throw "Rollback destination escaped root: $destination"}
 New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force|Out-Null
 Copy-Item -LiteralPath (Join-Path $original $entry.Key) -Destination $destination -Force
}
$created=@(
 'decompiled-src\CompleteCheatMenu\Targeting\InitialVelocityRedirector.cs',
 'decompiled-src\CompleteCheatMenu\Targeting\ProjectileSpawnRegistration.cs',
 'decompiled-src\CompleteCheatMenu\Patches\WeaponAddProjectile_Patch.cs',
 'decompiled-src\CompleteCheatMenu\Patches\WeaponAddProjectiles_Patch.cs',
 'tests\projectile_initial_velocity_test.py',
 'tests\projectile_initial_velocity_harness.cs',
 'tests\projectile_spawn_registration_harness.cs'
)
foreach($relative in $created){$path=[IO.Path]::GetFullPath((Join-Path $root $relative));if(-not $path.StartsWith($root,[StringComparison]::OrdinalIgnoreCase)){throw "Rollback deletion escaped root: $path"};if(Test-Path -LiteralPath $path -PathType Leaf){Remove-Item -LiteralPath $path -Force}}
if(-not[string]::IsNullOrWhiteSpace($DestinationDll)){$dll=[IO.Path]::GetFullPath($DestinationDll);New-Item -ItemType Directory -Path (Split-Path -Parent $dll) -Force|Out-Null;Copy-Item -LiteralPath $BaselineDll -Destination $dll -Force;Write-Output "RESTORED_DLL_SHA256=$((Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash)"}
Write-Output 'RESTORED_SOURCE_FILES=7'
Write-Output "CREATED_FILES_PRESENT=$(@($created|Where-Object{Test-Path -LiteralPath (Join-Path $root $_)-PathType Leaf}).Count)"
Write-Output 'ROLLBACK_TASK17_3=PASS'
