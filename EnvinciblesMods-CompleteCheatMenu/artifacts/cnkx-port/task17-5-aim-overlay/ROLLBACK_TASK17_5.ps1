[CmdletBinding()]
param(
  [string]$ProjectRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu',
  [string]$GameDll = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\BepInEx\plugins\EnvinciblesMods-CompleteCheatMenu\CompleteCheatMenu.dll'
)
$ErrorActionPreference='Stop'
$Here=Split-Path -Parent $MyInvocation.MyCommand.Path
$OriginalSource=Join-Path $Here 'original\source'
$OriginalDll=Join-Path $Here 'original\game\CompleteCheatMenu.dll'
$restore=@(
 'decompiled-src\CompleteCheatMenu\Runtime\EspRendererV2.cs',
 'decompiled-src\CompleteCheatMenu\Runtime\CheatState.cs',
 'decompiled-src\CompleteCheatMenu\Runtime\Presets.cs',
 'decompiled-src\CompleteCheatMenu\UI\Tabs\VisualsTab.cs',
 'tests\localized_ui_preset_test.py',
 'docs\plans\2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md'
)
foreach($rel in $restore){
 $src=Join-Path $OriginalSource $rel; $dst=Join-Path $ProjectRoot $rel
 New-Item -ItemType Directory -Force -Path (Split-Path -Parent $dst)|Out-Null
 Copy-Item -LiteralPath $src -Destination $dst -Force
}
@('decompiled-src\CompleteCheatMenu\Targeting\AimOverlayMath.cs','tests\aim_overlay_test.py','tests\aim_overlay_harness.cs') | ForEach-Object {
 $path=Join-Path $ProjectRoot $_; if(Test-Path -LiteralPath $path){Remove-Item -LiteralPath $path -Force}
}
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $GameDll)|Out-Null
Copy-Item -LiteralPath $OriginalDll -Destination $GameDll -Force
$expected='224A13AE043E203D3EBCBFF840241BB32588A7549B8F8DC8326236513A4AAB0A'
$actual=(Get-FileHash -Algorithm SHA256 -LiteralPath $GameDll).Hash
if($actual -ne $expected){throw "Rollback DLL hash mismatch: $actual"}
Write-Output "ROLLBACK_TASK17_5=PASS"
Write-Output "RESTORED_DLL_SHA256=$actual"
Write-Output "RESTORED_SOURCE_FILES=$($restore.Count)"
Write-Output "CREATED_FILES_PRESENT=$(@('decompiled-src\CompleteCheatMenu\Targeting\AimOverlayMath.cs','tests\aim_overlay_test.py','tests\aim_overlay_harness.cs') | Where-Object {Test-Path -LiteralPath (Join-Path $ProjectRoot $_)} | Measure-Object | Select-Object -ExpandProperty Count)"
