$ErrorActionPreference = 'Stop'
$game = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish'
$managed = Join-Path $game 'How to Fish_Data\Managed'
$core = Join-Path $game 'BepInEx\core'
$csc = 'D:\Code\How2fish\Kai935-FishAimbot\.tools\roslyn\tasks\net472\csc.exe'
$artifactDir = Join-Path $PSScriptRoot '..\artifacts'
New-Item -ItemType Directory -Path $artifactDir -Force | Out-Null
$output = Join-Path $artifactDir 'CompleteCheatMenu.dll'
$refs = @(
  (Join-Path $core 'BepInEx.dll'), (Join-Path $core '0Harmony.dll'),
  (Join-Path $managed 'Assembly-CSharp.dll'), (Join-Path $managed 'netstandard.dll'),
  (Join-Path $managed 'FishNet.Runtime.dll'), (Join-Path $managed 'UnityEngine.dll'),
  (Join-Path $managed 'UnityEngine.CoreModule.dll'), (Join-Path $managed 'UnityEngine.IMGUIModule.dll'),
  (Join-Path $managed 'UnityEngine.InputLegacyModule.dll'), (Join-Path $managed 'UnityEngine.PhysicsModule.dll'),
  (Join-Path $managed 'UnityEngine.TextRenderingModule.dll')
)
$args = @('/nologo','/target:library','/optimize+','/debug:pdbonly',"/out:$output")
$args += $refs | ForEach-Object { "/reference:$_" }
$args += Get-ChildItem $PSScriptRoot -Recurse -Filter *.cs | ForEach-Object FullName
& $csc @args
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Get-FileHash $output -Algorithm SHA256
