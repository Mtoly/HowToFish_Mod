param([string]$Source='CompleteCheatMenu.dll',[string]$Destination='CompleteCheatMenu.zh-CN.dll')
Copy-Item -LiteralPath $Source -Destination $Destination -Force
Get-FileHash -LiteralPath $Destination -Algorithm SHA256
