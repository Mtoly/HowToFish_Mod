param([Parameter(Mandatory)] [string] $PluginDll)

$ErrorActionPreference = 'Stop'
$Core = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\BepInEx\core'
Add-Type -LiteralPath "$Core\Mono.Cecil.dll"
$Assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($PluginDll)
try {
    $Type = $Assembly.MainModule.Types | Where-Object FullName -eq 'MorePlayers.CreateLobby_Patch'
    $Method = $Type.Methods | Where-Object Name -eq 'Transpiler'
    $Instructions = @($Method.Body.Instructions)
    $NewObjects = @($Instructions | Where-Object {
        $_.OpCode.Code -eq [Mono.Cecil.Cil.Code]::Newobj -and
        $_.Operand.DeclaringType.FullName -eq 'HarmonyLib.CodeInstruction'
    }).Count
    $OpcodeWrites = @($Instructions | Where-Object {
        $_.OpCode.Code -eq [Mono.Cecil.Cil.Code]::Stfld -and $_.Operand.Name -eq 'opcode'
    }).Count
    $OperandWrites = @($Instructions | Where-Object {
        $_.OpCode.Code -eq [Mono.Cecil.Cil.Code]::Stfld -and $_.Operand.Name -eq 'operand'
    }).Count
    Write-Output "PLUGIN=$PluginDll"
    Write-Output "NEW_CODEINSTRUCTION_COUNT=$NewObjects"
    Write-Output "OPCODE_IN_PLACE_WRITES=$OpcodeWrites"
    Write-Output "OPERAND_IN_PLACE_WRITES=$OperandWrites"
    if ($NewObjects -ne 0 -or $OpcodeWrites -ne 1 -or $OperandWrites -ne 1) { exit 1 }
    Write-Output 'RESULT=PASS_LABELS_PRESERVED_BY_IN_PLACE_MUTATION'
}
finally { $Assembly.Dispose() }
exit 0
