[CmdletBinding()]
param(
	[string]$ProjectRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu',
	[string]$GameDll = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\BepInEx\plugins\EnvinciblesMods-CompleteCheatMenu\CompleteCheatMenu.dll'
)

$ErrorActionPreference = 'Stop'
$original = Join-Path $PSScriptRoot 'original'
$restore = @{
	'tools\localize_dll.py' = 'tools__localize_dll.py'
	'decompiled-src\CompleteCheatMenu\UI\Tabs\GamblingTab.cs' = 'decompiled-src__CompleteCheatMenu__UI__Tabs__GamblingTab.cs'
	'docs\plans\2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md' = 'docs__plans__2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md'
}
foreach ($relative in $restore.Keys)
{
	$destination = Join-Path $ProjectRoot $relative
	New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
	Copy-Item -LiteralPath (Join-Path $original $restore[$relative]) -Destination $destination -Force
}
$created = Join-Path $ProjectRoot 'tests\gambling_colour_localization_test.py'
if (Test-Path -LiteralPath $created)
{
	Remove-Item -LiteralPath $created -Force
}
Copy-Item -LiteralPath (Join-Path $original 'CompleteCheatMenu.deployed.dll') -Destination $GameDll -Force
$actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $GameDll).Hash
$expected = 'A105A0E1AB2A67A252387833000BAFFFDE46E3B6FC3AA8A7A6A708CF7404D0E0'
if ($actual -ne $expected)
{
	throw "Rollback DLL hash mismatch: $actual"
}
Write-Output 'ROLLBACK_GAMBLING_FIX=PASS'
Write-Output "RESTORED_DLL_SHA256=$actual"
Write-Output "RESTORED_SOURCE_FILES=$($restore.Count)"
Write-Output "CREATED_FILES_PRESENT=$([int](Test-Path -LiteralPath $created))"
