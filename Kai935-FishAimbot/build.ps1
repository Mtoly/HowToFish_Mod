$ErrorActionPreference = 'Stop'

$game = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish'
$managed = Join-Path $game 'How to Fish_Data\Managed'
$core = Join-Path $game 'BepInEx\core'
$csc = Join-Path $PSScriptRoot '.tools\roslyn\tasks\net472\csc.exe'
$output = Join-Path $PSScriptRoot 'AimbotRevised.optimized.dll'
$bepInEx = Join-Path $core 'BepInEx.dll'
$harmony = Join-Path $core '0Harmony.dll'
$assemblyCSharp = Join-Path $managed 'Assembly-CSharp.dll'
$unityCore = Join-Path $managed 'UnityEngine.CoreModule.dll'
$unityInput = Join-Path $managed 'UnityEngine.InputLegacyModule.dll'
$unityEngine = Join-Path $managed 'UnityEngine.dll'
$netstandard = Join-Path $managed 'netstandard.dll'
$fishNet = Join-Path $managed 'FishNet.Runtime.dll'

& $csc /nologo /target:library /optimize+ /debug:pdbonly /out:$output `
    /reference:$bepInEx `
    /reference:$harmony `
    /reference:$assemblyCSharp `
    /reference:$unityCore `
    /reference:$unityInput `
    /reference:$unityEngine `
    /reference:$netstandard `
    /reference:$fishNet `
    (Join-Path $PSScriptRoot 'src\Plugin.cs') `
    (Join-Path $PSScriptRoot 'src\CreatureRegistryPatch.cs') `
    (Join-Path $PSScriptRoot 'src\FishTargeting.cs') `
    (Join-Path $PSScriptRoot 'src\AimAssistPatch.cs')

if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Get-FileHash $output -Algorithm SHA256
