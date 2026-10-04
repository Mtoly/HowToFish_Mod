[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu',
    [string]$DestinationDll = ''
)
$ErrorActionPreference = 'Stop'
$source = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task16-original-src'
$mapPath = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task16-files.json'
$resolved = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $resolved -PathType Container)) { throw "DestinationRoot missing: $resolved" }
$map = Get-Content -LiteralPath $mapPath -Raw | ConvertFrom-Json
foreach ($property in $map.PSObject.Properties) {
    $target = Join-Path $resolved ($property.Name -replace '/', '\')
    New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $source $property.Value) -Destination $target -Force
}
$created = @('tests\client_mode_capability_test.py','CompleteCheatMenu.client-mode.zh-CN.dll','artifacts\CompleteCheatMenu.client-mode.dll')
foreach ($relative in $created) { $target = Join-Path $resolved $relative; if (Test-Path -LiteralPath $target) { Remove-Item -LiteralPath $target -Force } }
if (-not [string]::IsNullOrWhiteSpace($DestinationDll)) {
    $dll = [IO.Path]::GetFullPath($DestinationDll)
    New-Item -ItemType Directory -Path (Split-Path -Parent $dll) -Force | Out-Null
    Copy-Item -LiteralPath 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\CompleteCheatMenu.pre-client-mode.dll' -Destination $dll -Force
    $dllHash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash
    Write-Output "RESTORED_DLL_SHA256=$dllHash"
    if ($dllHash -ne '41F5B3EACE8DAD48754561FD5C936026C9B991A323F92DB9A2849CD82CA1AE30') { exit 2 }
}
$remaining = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $resolved $_) })
Write-Output "RESTORED_SOURCE_FILES=$($map.PSObject.Properties.Name.Count)"
Write-Output "CREATED_REMAINING=$($remaining.Count)"
Write-Output "ROLLBACK_MATCH=$($remaining.Count -eq 0)"
if ($remaining.Count -ne 0) { exit 2 }
exit 0

