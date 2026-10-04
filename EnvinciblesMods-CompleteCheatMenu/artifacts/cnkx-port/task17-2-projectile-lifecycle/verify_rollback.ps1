$ErrorActionPreference = 'Stop'
$workspace = 'D:\Code\How2fish\EnvinciblesMods-CompleteCheatMenu'
$artifact = Join-Path $workspace 'artifacts\cnkx-port\task17-2-projectile-lifecycle'
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
Copy-Item -LiteralPath (Join-Path $artifact 'CompleteCheatMenu.task17-2.zh-CN.dll') -Destination $dll
& (Join-Path $artifact 'ROLLBACK_TASK17_2.ps1') -DestinationRoot $test -DestinationDll $dll |
    Tee-Object -FilePath (Join-Path $artifact 'rollback-ps-run.txt')

$map = [ordered]@{
    'decompiled-src__CompleteCheatMenu__Plugin.cs' = 'decompiled-src\CompleteCheatMenu\Plugin.cs'
    'decompiled-src__CompleteCheatMenu__Runtime__TickDriver.cs' = 'decompiled-src\CompleteCheatMenu\Runtime\TickDriver.cs'
    'decompiled-src__CompleteCheatMenu__UI__Tabs__DiagnosticsTab.cs' = 'decompiled-src\CompleteCheatMenu\UI\Tabs\DiagnosticsTab.cs'
    'docs__plans__2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md' = 'docs\plans\2026-08-25-complete-cheat-menu-cnkx-port-implementation-plan.md'
}
foreach ($entry in $map.GetEnumerator()) {
    $expected = (Get-FileHash -LiteralPath (Join-Path $artifact ('original\' + $entry.Key)) -Algorithm SHA256).Hash
    $actual = (Get-FileHash -LiteralPath (Join-Path $test $entry.Value) -Algorithm SHA256).Hash
    Write-Output "SOURCE_MATCH $($entry.Value)=$($expected -eq $actual)"
    if ($expected -ne $actual) { throw "Source rollback mismatch: $($entry.Value)" }
}
$created = @(
    'decompiled-src\CompleteCheatMenu\Targeting\ProjectileBindings.cs',
    'decompiled-src\CompleteCheatMenu\Targeting\ProjectileOwnership.cs',
    'decompiled-src\CompleteCheatMenu\Targeting\ProjectileTracker.cs',
    'decompiled-src\CompleteCheatMenu\Targeting\MagicShotContext.cs',
    'tests\projectile_lifecycle_test.py',
    'tests\projectile_lifecycle_harness.cs'
)
$present = @($created | Where-Object { Test-Path -LiteralPath (Join-Path $test $_) -PathType Leaf })
Write-Output "CREATED_FILES_PRESENT=$($present.Count)"
if ($present.Count -ne 0) { throw 'Created Task 17.2 files remain after rollback.' }

Push-Location $test
try {
    foreach ($case in Get-ChildItem '.\tests' -Filter '*_test.py' | Sort-Object Name) {
        $output = & python $case.FullName 2>&1
        Write-Output "$($case.Name): $($output -join ' | ') EXIT=$LASTEXITCODE"
        if ($LASTEXITCODE -ne 0) { throw "Rollback regression failed: $($case.Name)" }
    }
    $raw = Join-Path $test 'rollback-task17-1.raw.dll'
    $localized = Join-Path $test 'rollback-task17-1.zh-CN.dll'
    & '.\decompiled-src\build.ps1' -GameRoot 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish' -OutputPath $raw
    if ($LASTEXITCODE -ne 0) { throw 'Rollback build failed.' }
    python '.\tools\localize_dll.py' $raw $localized
    if ($LASTEXITCODE -ne 0) { throw 'Rollback localization failed.' }
}
finally {
    Pop-Location
}
$rawHash = (Get-FileHash -LiteralPath $raw -Algorithm SHA256).Hash
$localizedHash = (Get-FileHash -LiteralPath $localized -Algorithm SHA256).Hash
Write-Output "ROLLBACK_RAW_SHA256=$rawHash"
Write-Output "ROLLBACK_LOCALIZED_SHA256=$localizedHash"
$restoredDllHash = (Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash
Write-Output "RESTORED_BASELINE_DLL_SHA256=$restoredDllHash"
Write-Output 'REBUILD_HASH_NOTE=absolute CodeView/PDB path changes the rebuilt DLL hash in an independent directory'
if ($restoredDllHash -ne 'ED9F679FD8AE6DCF12A0171A01252DBB2CF0A3CF5C63DD2E02A4FAD6E9192B6B') {
    throw 'Restored baseline DLL does not match Task 17.1.'
}
Write-Output 'ROLLBACK_PS_VERIFICATION=PASS'
