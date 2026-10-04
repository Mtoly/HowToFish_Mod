[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)][string]$ModifiedFile,
  [Parameter(Mandatory=$true)][string]$BaselineFile
)
$ErrorActionPreference='Stop'
$modified=(Resolve-Path -LiteralPath $ModifiedFile).Path
$baseline=(Resolve-Path -LiteralPath $BaselineFile).Path
if ((Split-Path -Parent $modified) -ne (Split-Path -Parent $baseline)) { throw 'Rollback inputs must share a test directory.' }
Copy-Item -LiteralPath $baseline -Destination $modified -Force
$actual=(Get-FileHash -LiteralPath $modified -Algorithm SHA256).Hash
$expected=(Get-FileHash -LiteralPath $baseline -Algorithm SHA256).Hash
if($actual -ne $expected){ throw "Rollback hash mismatch: $actual != $expected" }
'ROLLBACK_PS_VERIFICATION=PASS'
"RESTORED_DLL_SHA256=$actual"
