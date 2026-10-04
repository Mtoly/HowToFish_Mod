$ErrorActionPreference='Stop'
$Root='D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
$Out=Join-Path $Root 'artifacts\cnkx-port\task17-5-aim-overlay'
$Suffix=Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$Rels=@(
 'decompiled-src\CompleteCheatMenu\Runtime\EspRendererV2.cs',
 'decompiled-src\CompleteCheatMenu\Runtime\CheatState.cs',
 'decompiled-src\CompleteCheatMenu\Runtime\Presets.cs',
 'decompiled-src\CompleteCheatMenu\UI\Tabs\VisualsTab.cs',
 'tests\localized_ui_preset_test.py',
 'docs\plans\2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md',
 'decompiled-src\CompleteCheatMenu\Targeting\AimOverlayMath.cs',
 'tests\aim_overlay_test.py',
 'tests\aim_overlay_harness.cs'
)
function New-Fixture([string]$Dir){
 New-Item -ItemType Directory -Force -Path $Dir|Out-Null
 foreach($Rel in $Rels){
  $Dst=Join-Path $Dir $Rel
  New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Dst)|Out-Null
  Copy-Item -LiteralPath (Join-Path $Root $Rel) -Destination $Dst -Force
 }
 $Dll=Join-Path $Dir 'game\CompleteCheatMenu.dll'
 New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Dll)|Out-Null
 Copy-Item -LiteralPath (Join-Path $Out 'CompleteCheatMenu.task17-5.zh-CN.dll') -Destination $Dll -Force
 return $Dll
}
function Assert-Restored([string]$Dir,[string]$Dll){
 $Orig=Join-Path $Out 'original\source'
 foreach($Rel in $Rels[0..5]){
  $Expected=(Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $Orig $Rel)).Hash
  $Actual=(Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $Dir $Rel)).Hash
  if($Expected -ne $Actual){throw "SOURCE_ROLLBACK_MISMATCH=$Rel"}
 }
 foreach($Rel in $Rels[6..8]){if(Test-Path -LiteralPath (Join-Path $Dir $Rel)){throw "CREATED_FILE_REMAINS=$Rel"}}
 $Hash=(Get-FileHash -Algorithm SHA256 -LiteralPath $Dll).Hash
 if($Hash -ne '224A13AE043E203D3EBCBFF840241BB32588A7549B8F8DC8326236513A4AAB0A'){throw "DLL_ROLLBACK_MISMATCH=$Hash"}
 return $Hash
}
$PsDir=Join-Path $Out ("rollback-test-ps-"+$Suffix)
$PsDll=New-Fixture $PsDir
& (Join-Path $Out 'ROLLBACK_TASK17_5.ps1') -ProjectRoot $PsDir -GameDll $PsDll | Tee-Object -FilePath (Join-Path $Out 'rollback-ps-run.txt')
$PsHash=Assert-Restored $PsDir $PsDll
Write-Output "ROLLBACK_PS_VERIFICATION=PASS SHA256=$PsHash DIR=$PsDir"
$ShDir=Join-Path $Out ("rollback-test-sh-"+$Suffix)
$ShDll=New-Fixture $ShDir
$ShDirWsl='/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task17-5-aim-overlay/'+(Split-Path -Leaf $ShDir)
$ShDllWsl=$ShDirWsl+'/game/CompleteCheatMenu.dll'
$ShScriptWsl='/mnt/d/Code/How2fish/EnvinciblesMods-CompleteCheatMenu/artifacts/cnkx-port/task17-5-aim-overlay/ROLLBACK_TASK17_5.sh'
& bash $ShScriptWsl $ShDirWsl $ShDllWsl | Tee-Object -FilePath (Join-Path $Out 'rollback-sh-run.txt')
if($LASTEXITCODE -ne 0){throw "BASH_ROLLBACK_EXIT=$LASTEXITCODE"}
$ShHash=Assert-Restored $ShDir $ShDll
Write-Output "ROLLBACK_SH_VERIFICATION=PASS SHA256=$ShHash DIR=$ShDir"
