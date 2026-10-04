[CmdletBinding()]
param(
    [string]$Destination = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\decompiled-src\CompleteCheatMenu\Game\GameBinder.cs'
)

$ErrorActionPreference = 'Stop'
$Source = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\original-src\GameBinder.cs'
$Expected = '161300E1A8A5AAD437171B43A4FEF012222D86748C09278EFA6DF2F4BBACCDAC'
Copy-Item -LiteralPath $Source -Destination $Destination -Force
$Actual = (Get-FileHash -Algorithm SHA256 -LiteralPath $Destination).Hash
Write-Output "ROLLBACK_SHA256=$Actual"
Write-Output "ROLLBACK_MATCH=$($Actual -eq $Expected)"
if ($Actual -ne $Expected) { exit 2 }
