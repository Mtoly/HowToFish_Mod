[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)

$ErrorActionPreference = 'Stop'
$files = @(
    'tests\targeting_math_test.py',
    'tests\ballistic_prediction_test.py',
    'tests\tracking_probability_test.py'
)
foreach ($relative in $files) {
    $target = Join-Path $DestinationRoot $relative
    if (Test-Path -LiteralPath $target) {
        Remove-Item -LiteralPath $target -Force
    }
}
$remaining = @($files | Where-Object { Test-Path -LiteralPath (Join-Path $DestinationRoot $_) })
Write-Output "ROLLBACK_REMOVED=$($files.Count - $remaining.Count)"
Write-Output "ROLLBACK_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$($remaining.Count -eq 0)"
if ($remaining.Count -ne 0) { exit 2 }
