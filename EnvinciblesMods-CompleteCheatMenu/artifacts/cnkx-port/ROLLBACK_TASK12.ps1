[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)

$ErrorActionPreference = 'Stop'
$SourceRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task12-original-src'
$resolved = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
    throw "DestinationRoot does not exist: $resolved"
}
$restore = [ordered]@{
    'EspRenderer.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\EspRenderer.cs'
    'CheatState.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\CheatState.cs'
    'Plugin.cs' = 'decompiled-src\CompleteCheatMenu\Plugin.cs'
}
$created = @(
    'decompiled-src\CompleteCheatMenu\Runtime\EspRendererV2.cs',
    'decompiled-src\CompleteCheatMenu\Runtime\GuiPrimitives.cs',
    'tests\cnkx_esp_projection_impl.py',
    'tests\esp_projection_test.py'
)
foreach ($entry in $restore.GetEnumerator()) {
    $destination = Join-Path $resolved $entry.Value
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
    Copy-Item -LiteralPath (Join-Path $SourceRoot $entry.Key) -Destination $destination -Force
}
foreach ($relative in $created) {
    $target = Join-Path $resolved $relative
    if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Force }
}
$espHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['EspRenderer.cs'])).Hash
$cheatHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['CheatState.cs'])).Hash
$pluginHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['Plugin.cs'])).Hash
$remaining = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $resolved $_) })
$match = $espHash -eq '122A3AEBB550BC294C18710DC6FD2E9CD3975CB4C7DDDC52F99FA12FD296D87D' -and $cheatHash -eq 'E028F72E3B5EDDC98786DBD89D322CD7B2DAA5F44C763ECC4D092284AF78640D' -and $pluginHash -eq 'DAF9983F40DCAFCD2EFB936D346EABDEBFFCAC088E5F208E459D79594FA45BEC' -and $remaining.Count -eq 0
Write-Output "ESP_RENDERER_SHA256=$espHash"
Write-Output "CHEATSTATE_SHA256=$cheatHash"
Write-Output "PLUGIN_SHA256=$pluginHash"
Write-Output "CREATED_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$match"
if (-not $match) { exit 2 }
