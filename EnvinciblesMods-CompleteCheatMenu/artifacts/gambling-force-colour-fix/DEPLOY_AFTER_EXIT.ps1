[CmdletBinding()]
param(
	[int]$GameProcessId = 66320,
	[string]$SourceDll = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\gambling-force-colour-fix\CompleteCheatMenu.gambling-fix.zh-CN.dll',
	[string]$GameDll = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\BepInEx\plugins\EnvinciblesMods-CompleteCheatMenu\CompleteCheatMenu.dll'
)

$ErrorActionPreference = 'Stop'
$statusFile = Join-Path $PSScriptRoot 'deployment-after-exit.txt'
function Get-Sha256([string]$Path)
{
	$stream = [IO.File]::OpenRead($Path)
	try
	{
		$sha = [Security.Cryptography.SHA256]::Create()
		try
		{
			return ([BitConverter]::ToString($sha.ComputeHash($stream))).Replace('-', '')
		}
		finally
		{
			$sha.Dispose()
		}
	}
	finally
	{
		$stream.Dispose()
	}
}
try
{
	if (Get-Process -Id $GameProcessId -ErrorAction SilentlyContinue)
	{
		Wait-Process -Id $GameProcessId
	}
	Copy-Item -LiteralPath $SourceDll -Destination $GameDll -Force
	$expected = Get-Sha256 $SourceDll
	$actual = Get-Sha256 $GameDll
	if ($actual -ne $expected)
	{
		throw "Deployment hash mismatch: expected $expected, got $actual"
	}
	@(
		'DEPLOY_AFTER_EXIT=PASS',
		"GAME_PROCESS_ID=$GameProcessId",
		"DEPLOYED_DLL=$GameDll",
		"DEPLOYED_SHA256=$actual"
	) | Set-Content -LiteralPath $statusFile -Encoding utf8
}
catch
{
	@(
		'DEPLOY_AFTER_EXIT=FAIL',
		"GAME_PROCESS_ID=$GameProcessId",
		"ERROR=$($_.Exception.Message)"
	) | Set-Content -LiteralPath $statusFile -Encoding utf8
	exit 1
}
