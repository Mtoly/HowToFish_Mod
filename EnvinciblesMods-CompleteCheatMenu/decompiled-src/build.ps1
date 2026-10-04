[CmdletBinding()]
param(
  [string]$GameRoot = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish',
  [string]$OutputPath = ''
)

$ErrorActionPreference = 'Stop'
$game = [IO.Path]::GetFullPath($GameRoot)
$managed = Join-Path $game 'How to Fish_Data\Managed'
$core = Join-Path $game 'BepInEx\core'
$csc = 'D:\Code\How2fish\Kai935-FishAimbot\.tools\roslyn\tasks\net472\csc.exe'
$artifactDir = Join-Path $PSScriptRoot '..\artifacts'
New-Item -ItemType Directory -Path $artifactDir -Force | Out-Null
$output = if ([string]::IsNullOrWhiteSpace($OutputPath)) {
  Join-Path $artifactDir 'CompleteCheatMenu.dll'
} else {
  [IO.Path]::GetFullPath($OutputPath)
}
New-Item -ItemType Directory -Path (Split-Path -Parent $output) -Force | Out-Null
$refs = @(
  (Join-Path $core 'BepInEx.dll'), (Join-Path $core '0Harmony.dll'),
  (Join-Path $managed 'Assembly-CSharp.dll'), (Join-Path $managed 'netstandard.dll'),
  (Join-Path $managed 'FishNet.Runtime.dll'), (Join-Path $managed 'UnityEngine.dll'),
  (Join-Path $managed 'UnityEngine.CoreModule.dll'), (Join-Path $managed 'UnityEngine.IMGUIModule.dll'),
  (Join-Path $managed 'UnityEngine.AnimationModule.dll'),
  (Join-Path $managed 'UnityEngine.InputLegacyModule.dll'), (Join-Path $managed 'UnityEngine.PhysicsModule.dll'),
  (Join-Path $managed 'UnityEngine.TextRenderingModule.dll')
)
$missing = @($refs | Where-Object { -not (Test-Path -LiteralPath $_ -PathType Leaf) })
if ($missing.Count -gt 0) {
  throw "Missing build reference(s): $($missing -join ', ')"
}
$args = @('/nologo','/target:library','/optimize+','/debug:pdbonly',"/out:$output")
$args += $refs | ForEach-Object { "/reference:$_" }
$sources = @(Get-ChildItem $PSScriptRoot -Recurse -Filter *.cs | Sort-Object FullName | ForEach-Object FullName)
$args += $sources
& $csc @args
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$hash = (Get-FileHash -LiteralPath $output -Algorithm SHA256).Hash
Write-Output "BUILD_OUTPUT=$output"
Write-Output "BUILD_SOURCE_COUNT=$($sources.Count)"
Write-Output "BUILD_SHA256=$hash"
