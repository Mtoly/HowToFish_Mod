[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)

$ErrorActionPreference = 'Stop'
$SourceRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task5-original-src'
$resolved = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
    throw "DestinationRoot does not exist: $resolved"
}
$restore = [ordered]@{
    'CheatState.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\CheatState.cs'
    'targeting_math_test.py' = 'tests\targeting_math_test.py'
}
$created = @(
    'decompiled-src\CompleteCheatMenu\Targeting\TargetCandidate.cs',
    'decompiled-src\CompleteCheatMenu\Targeting\TargetSolution.cs',
    'decompiled-src\CompleteCheatMenu\Targeting\TargetingSettings.cs',
    'decompiled-src\CompleteCheatMenu\Targeting\TargetScorer.cs',
    'tests\cnkx_math_impl.py'
)
foreach ($entry in $restore.GetEnumerator()) {
    $destination = Join-Path $resolved $entry.Value
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
    Copy-Item -LiteralPath (Join-Path $SourceRoot $entry.Key) -Destination $destination -Force
}
foreach ($relative in $created) {
    $target = Join-Path $resolved $relative
    if (Test-Path -LiteralPath $target) {
        Remove-Item -LiteralPath $target -Force
    }
}
$cheatHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['CheatState.cs'])).Hash
$testHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['targeting_math_test.py'])).Hash
$remaining = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $resolved $_) })
$match = $cheatHash -eq 'B3FCCAAF0C3B64CD036667F3E762C6831C14D710D652B8E883A410047AA6A553' -and $testHash -eq '96B18C0F3D0810B44E809F633E0682F616C5FBD6615BFF40CCBDCB76AA7D9C0F' -and $remaining.Count -eq 0
Write-Output "CHEATSTATE_SHA256=$cheatHash"
Write-Output "TARGETING_TEST_SHA256=$testHash"
Write-Output "CREATED_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$match"
if (-not $match) { exit 2 }
