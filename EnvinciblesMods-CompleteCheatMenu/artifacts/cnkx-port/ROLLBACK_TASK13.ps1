[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)

$ErrorActionPreference = 'Stop'
$SourceRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task13-original-src'
$resolved = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $resolved -PathType Container)) { throw "DestinationRoot does not exist: $resolved" }
$restore = [ordered]@{
    'VisualsTab.cs' = 'decompiled-src\CompleteCheatMenu\UI\Tabs\VisualsTab.cs'
    'WeaponsTab.cs' = 'decompiled-src\CompleteCheatMenu\UI\Tabs\WeaponsTab.cs'
    'DiagnosticsTab.cs' = 'decompiled-src\CompleteCheatMenu\UI\Tabs\DiagnosticsTab.cs'
    'MenuWindow.cs' = 'decompiled-src\CompleteCheatMenu\UI\MenuWindow.cs'
    'Presets.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\Presets.cs'
    'CheatState.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\CheatState.cs'
    'TargetingSystem.cs' = 'decompiled-src\CompleteCheatMenu\Targeting\TargetingSystem.cs'
    'localize_dll.py' = 'tools\localize_dll.py'
}
$expected = @{
    'VisualsTab.cs' = 'B90A3BCF72CF6E4D7F06C2BAC706510BC53E82B6277133664759301D232DC1B9'
    'WeaponsTab.cs' = '19D9977FC059932195AD42BF5EBC1A850312AC946B039B16C34E45EA9D9324D3'
    'DiagnosticsTab.cs' = '299F27FEFF318127AE522704E0CF9203401FB97D6290D996EC43151E55811228'
    'MenuWindow.cs' = 'E748C37B3CC8FE8C69FF0D35FEA595D7FD3B0D9387565A7AE20D60315FEBDF90'
    'Presets.cs' = '46558DFB351DFC7B7D466706238A79B1A2D6BCD353FD0E2ED0BB57D34711688C'
    'CheatState.cs' = '1D1C0F4A86F8E5372958E5892C2B09864312A32705802E6D858E2261A6520F19'
    'TargetingSystem.cs' = 'DA694AE460A3D6663D5AB1CE311D0F40C657D157518C46A92603EF058386566D'
    'localize_dll.py' = 'C0AF213B0BDA2B4DB59FA2BE3974C99BA61A6B569E19BB8C5CAAB0539A32EA8A'
}
foreach ($entry in $restore.GetEnumerator()) {
    $destination = Join-Path $resolved $entry.Value
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
    Copy-Item -LiteralPath (Join-Path $SourceRoot $entry.Key) -Destination $destination -Force
}
$created = @('tests\localized_ui_preset_test.py')
foreach ($relative in $created) { $target = Join-Path $resolved $relative; if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Force } }
$match = $true
foreach ($entry in $restore.GetEnumerator()) {
    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $entry.Value)).Hash
    Write-Output "$($entry.Key)_SHA256=$hash"
    if ($hash -ne $expected[$entry.Key]) { $match = $false }
}
$remaining = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $resolved $_) })
Write-Output "CREATED_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$match"
if (-not $match -or $remaining.Count -ne 0) { exit 2 }
