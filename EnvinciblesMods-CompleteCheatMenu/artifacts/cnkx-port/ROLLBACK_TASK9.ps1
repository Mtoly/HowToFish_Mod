[CmdletBinding()]
param(
    [string]$DestinationRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
)

$ErrorActionPreference = 'Stop'
$SourceRoot = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu\artifacts\cnkx-port\task9-original-src'
$resolved = [IO.Path]::GetFullPath($DestinationRoot)
if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
    throw "DestinationRoot does not exist: $resolved"
}
$restore = [ordered]@{
    'WeaponShoot_Patch.cs' = 'decompiled-src\CompleteCheatMenu\Patches\WeaponShoot_Patch.cs'
    'WeaponCheats.cs' = 'decompiled-src\CompleteCheatMenu\Cheats\WeaponCheats.cs'
    'CheatState.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\CheatState.cs'
    'cnkx_math_impl.py' = 'tests\cnkx_math_impl.py'
    'tracking_probability_test.py' = 'tests\tracking_probability_test.py'
}
foreach ($entry in $restore.GetEnumerator()) {
    $destination = Join-Path $resolved $entry.Value
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $destination) | Out-Null
    Copy-Item -LiteralPath (Join-Path $SourceRoot $entry.Key) -Destination $destination -Force
}
$created = 'decompiled-src\CompleteCheatMenu\Targeting\ShotRedirector.cs'
$createdPath = Join-Path $resolved $created
if (Test-Path -LiteralPath $createdPath) { Remove-Item -LiteralPath $createdPath -Force }
$patchHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['WeaponShoot_Patch.cs'])).Hash
$weaponHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['WeaponCheats.cs'])).Hash
$cheatHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['CheatState.cs'])).Hash
$mathHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['cnkx_math_impl.py'])).Hash
$testHash = (Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $resolved $restore['tracking_probability_test.py'])).Hash
$remaining = [int](Test-Path -LiteralPath $createdPath)
$match = $patchHash -eq 'B83ED4F0E6C0EE4F35E03F7B8259A87F9E9AC01B3B081D6F7E0364527F0F16B3' -and $weaponHash -eq 'B99444C00D46140CE35A73251610000F3C5D718489F23149AA894B8C705310C6' -and $cheatHash -eq '3DE73E553C2DC03615E73453A2C088F8F5484E560E6AADC5E585642224F5C386' -and $mathHash -eq 'DD15D856193D6C8F18D3BF221ED7FBFD7921FED7BBDA9AFDF841A53F19728EED' -and $testHash -eq '5CA9A34E3248527DDB739D6E2B4A0EAF6847FA9153667A795B058D6940352608' -and $remaining -eq 0
Write-Output "SHOOT_PATCH_SHA256=$patchHash"
Write-Output "WEAPON_SHA256=$weaponHash"
Write-Output "CHEATSTATE_SHA256=$cheatHash"
Write-Output "MATH_IMPL_SHA256=$mathHash"
Write-Output "TRACKING_TEST_SHA256=$testHash"
Write-Output "CREATED_REMAINING=$remaining"
Write-Output "ROLLBACK_MATCH=$match"
if (-not $match) { exit 2 }
