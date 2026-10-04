[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)

$ErrorActionPreference = 'Stop'
$SourceRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task6-original-src'
$resolved = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
    throw "DestinationRoot does not exist: $resolved"
}
$buildPath = Join-Path $resolved 'decompiled-src\build.ps1'
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $buildPath) | Out-Null
Copy-Item -LiteralPath (Join-Path $SourceRoot 'build.ps1') -Destination $buildPath -Force
$created = @(
    'decompiled-src\CompleteCheatMenu\Targeting\AimPoint.cs',
    'decompiled-src\CompleteCheatMenu\Targeting\BoneResolver.cs',
    'decompiled-src\CompleteCheatMenu\Targeting\RendererBoundsResolver.cs',
    'tests\cnkx_bone_impl.py',
    'tests\bone_selection_test.py'
)
foreach ($relative in $created) {
    $target = Join-Path $resolved $relative
    if (Test-Path -LiteralPath $target) {
        Remove-Item -LiteralPath $target -Force
    }
}
$buildHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $buildPath).Hash
$remaining = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $resolved $_) })
$match = $buildHash -eq '894DA10D6AF69655BCC47AAF900CCB479B3577D1502DE63EADF10D8D91087253' -and $remaining.Count -eq 0
Write-Output "BUILD_PS1_SHA256=$buildHash"
Write-Output "CREATED_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$match"
if (-not $match) { exit 2 }
