param([Parameter(Mandatory=$true)][string]$Target)
$resolved=(Resolve-Path -LiteralPath $Target).Path
if((Split-Path $resolved -Leaf) -ne 'AlwaysSprint.dll'){ throw 'Target must be AlwaysSprint.dll' }
Remove-Item -LiteralPath $resolved -Force
$dir=Split-Path $resolved -Parent
if((Get-ChildItem -LiteralPath $dir -Force | Measure-Object).Count -eq 0){Remove-Item -LiteralPath $dir -Force}
Write-Output 'rollback=removed-new-plugin'
