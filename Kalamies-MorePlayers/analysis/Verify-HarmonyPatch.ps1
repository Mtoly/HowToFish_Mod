param([Parameter(Mandatory)] [string] $PluginDll)

$ErrorActionPreference = 'Stop'
$Game = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish'
$Core = "$Game\BepInEx\core"
$Managed = "$Game\How to Fish_Data\Managed"

$SearchDirectories = @($Core, $Managed, (Split-Path -Parent $PluginDll))
$Handler = [ResolveEventHandler] {
    param($Sender, $EventArgs)
    $Name = ([Reflection.AssemblyName]$EventArgs.Name).Name + '.dll'
    foreach ($Directory in $SearchDirectories) {
        $Candidate = Join-Path $Directory $Name
        if (Test-Path -LiteralPath $Candidate) { return [Reflection.Assembly]::LoadFrom($Candidate) }
    }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($Handler)

try {
    $HarmonyAssembly = [Reflection.Assembly]::LoadFrom("$Core\0Harmony.dll")
    [void][Reflection.Assembly]::LoadFrom("$Core\BepInEx.dll")
    [void][Reflection.Assembly]::LoadFrom("$Managed\Assembly-CSharp.dll")
    $PluginAssembly = [Reflection.Assembly]::LoadFrom((Resolve-Path -LiteralPath $PluginDll))
    $HarmonyType = $HarmonyAssembly.GetType('HarmonyLib.Harmony', $true)
    $Harmony = [Activator]::CreateInstance($HarmonyType, @("moreplayers.verify.$([Guid]::NewGuid())"))
    $PatchAll = $HarmonyType.GetMethods() | Where-Object {
        $_.Name -eq 'PatchAll' -and
        $_.GetParameters().Count -eq 1 -and
        $_.GetParameters()[0].ParameterType -eq [Reflection.Assembly]
    } | Select-Object -First 1
    if ($null -eq $PatchAll) { throw 'Harmony.PatchAll(Assembly) was not found.' }
    try {
        [void]$PatchAll.Invoke($Harmony, @($PluginAssembly))
        Write-Output "PLUGIN=$PluginDll"
        Write-Output 'RESULT=PASS_HARMONY_PATCHALL'
        exit 0
    }
    catch {
        Write-Output "ERROR_DETAIL=$($_.Exception.ToString().Replace("`r", ' ').Replace("`n", ' | '))"
        $ErrorObject = $_.Exception
        while ($ErrorObject.InnerException) { $ErrorObject = $ErrorObject.InnerException }
        Write-Output "PLUGIN=$PluginDll"
        Write-Output "RESULT=FAIL_HARMONY_PATCHALL"
        Write-Output "ERROR_TYPE=$($ErrorObject.GetType().FullName)"
        Write-Output "ERROR_MESSAGE=$($ErrorObject.Message)"
        exit 1
    }
}
finally {
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($Handler)
}
