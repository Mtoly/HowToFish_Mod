$ErrorActionPreference = 'Stop'
$workspace = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
$artifact = Join-Path $workspace 'artifacts\cnkx-port\task17-3-initial-velocity'
$test = [IO.Path]::GetFullPath((Join-Path $artifact 'rollback-test-ps-20260825'))
if (-not $test.StartsWith([IO.Path]::GetFullPath($artifact), [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Rollback test path escaped artifact directory.'
}
if (Test-Path -LiteralPath $test) {
    Remove-Item -LiteralPath $test -Recurse -Force
}
New-Item -ItemType Directory -Path $test | Out-Null
foreach ($directory in @('decompiled-src', 'docs', 'tests', 'tools')) {
    Copy-Item -LiteralPath (Join-Path $workspace $directory) -Destination $test -Recurse
}
$dll = Join-Path $test 'deployed.dll'
Copy-Item -LiteralPath (Join-Path $artifact 'CompleteCheatMenu.task17-3.zh-CN.dll') -Destination $dll
& (Join-Path $artifact 'ROLLBACK_TASK17_3.ps1') -DestinationRoot $test -DestinationDll $dll |
    Tee-Object -FilePath (Join-Path $artifact 'rollback-ps-run.txt')

$map = [ordered]@{
    'decompiled-src__CompleteCheatMenu__Patches__WeaponShoot_Patch.cs' = 'decompiled-src\CompleteCheatMenu\Patches\WeaponShoot_Patch.cs'
    'decompiled-src__CompleteCheatMenu__Cheats__WeaponCheats.cs' = 'decompiled-src\CompleteCheatMenu\Cheats\WeaponCheats.cs'
    'decompiled-src__CompleteCheatMenu__Targeting__ProjectileBindings.cs' = 'decompiled-src\CompleteCheatMenu\Targeting\ProjectileBindings.cs'
    'decompiled-src__CompleteCheatMenu__Targeting__MagicShotContext.cs' = 'decompiled-src\CompleteCheatMenu\Targeting\MagicShotContext.cs'
    'tests__tracking_probability_test.py' = 'tests\tracking_probability_test.py'
    'tests__projectile_lifecycle_harness.cs' = 'tests\projectile_lifecycle_harness.cs'
    'docs__plans__2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md' = 'docs\plans\2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md'
}
foreach ($entry in $map.GetEnumerator()) {
    $expected = (Get-FileHash -LiteralPath (Join-Path $artifact ('original\' + $entry.Key)) -Algorithm SHA256).Hash
    $actual = (Get-FileHash -LiteralPath (Join-Path $test $entry.Value) -Algorithm SHA256).Hash
    Write-Output "SOURCE_MATCH $($entry.Value)=$($expected -eq $actual)"
    if ($expected -ne $actual) { throw "Source rollback mismatch: $($entry.Value)" }
}
$created = @(
    'decompiled-src\CompleteCheatMenu\Targeting\InitialVelocityRedirector.cs',
    'decompiled-src\CompleteCheatMenu\Targeting\ProjectileSpawnRegistration.cs',
    'decompiled-src\CompleteCheatMenu\Patches\WeaponAddProjectile_Patch.cs',
    'decompiled-src\CompleteCheatMenu\Patches\WeaponAddProjectiles_Patch.cs',
    'tests\projectile_initial_velocity_test.py',
    'tests\projectile_initial_velocity_harness.cs',
    'tests\projectile_spawn_registration_harness.cs'
)
$present = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $test $_) -PathType Leaf })
Write-Output "CREATED_FILES_PRESENT=$($present.Count)"
if ($present.Count -ne 0) { throw 'Created Task 17.3 files remain after rollback.' }

Push-Location $test
try {
    foreach ($case in Get-ChildItem '.\tests' -Filter '*_test.py' | Sort-Object Name) {
        $output = & python $case.FullName 2>&1
        Write-Output "$($case.Name): $($output -join ' | ') EXIT=$LASTEXITCODE"
        if ($LASTEXITCODE -ne 0) { throw "Rollback regression failed: $($case.Name)" }
    }
    $csc = 'D:\Code\How2fish\Kai935-FishAimbot\.tools\roslyn\tasks\net472\csc.exe'
    $life = Join-Path $test 'projectile_lifecycle_harness.exe'
    & $csc /nologo /target:exe /optimize+ "/out:$life" '.\tests\projectile_lifecycle_harness.cs' '.\decompiled-src\CompleteCheatMenu\Targeting\ProjectileOwnership.cs' '.\decompiled-src\CompleteCheatMenu\Targeting\ProjectileTracker.cs' '.\decompiled-src\CompleteCheatMenu\Targeting\MagicShotContext.cs'
    if ($LASTEXITCODE -ne 0) { throw 'Rollback lifecycle harness build failed.' }
    & $life
    if ($LASTEXITCODE -ne 0) { throw 'Rollback lifecycle harness failed.' }
    $raw = Join-Path $test 'rollback-task17-2.raw.dll'
    & '.\decompiled-src\build.ps1' -GameRoot 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish' -OutputPath $raw
    if ($LASTEXITCODE -ne 0) { throw 'Rollback production build failed.' }
}
finally {
    Pop-Location
}
$restoredDllHash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash
Write-Output "RESTORED_BASELINE_DLL_SHA256=$restoredDllHash"
if ($restoredDllHash -ne '9361B9E43A3C77E9E32FD5421C23E51739BB569A099538DA024B5877E7558DB2') {
    throw 'Restored baseline DLL does not match Task 17.2.'
}
Write-Output 'ROLLBACK_PS_VERIFICATION=PASS'
