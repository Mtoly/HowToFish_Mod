param(
    [string] $InputDll = 'D:\Code\How2fish\Kalamies-MorePlayers\MorePlayers.dll',
    [string] $OutputDll = 'D:\Code\How2fish\Kalamies-MorePlayers\build\MorePlayers.fixed.dll'
)

$ErrorActionPreference = 'Stop'
$Core = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\BepInEx\core'
Add-Type -LiteralPath "$Core\Mono.Cecil.dll"

$Resolver = [Mono.Cecil.DefaultAssemblyResolver]::new()
$Resolver.AddSearchDirectory($Core)
$Resolver.AddSearchDirectory((Split-Path -Parent $InputDll))
$Reader = [Mono.Cecil.ReaderParameters]::new()
$Reader.AssemblyResolver = $Resolver
$Assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($InputDll, $Reader)

try {
    $Module = $Assembly.MainModule
    $Type = $Module.Types | Where-Object FullName -eq 'MorePlayers.CreateLobby_Patch'
    $Method = $Type.Methods | Where-Object Name -eq 'Transpiler'
    if ($null -eq $Method) { throw 'Transpiler method was not found.' }

    $Instructions = $Method.Body.Instructions
    $Start = $Instructions | Where-Object Offset -eq 0x0060
    $End = $Instructions | Where-Object Offset -eq 0x0082
    if ($null -eq $Start -or $null -eq $End) { throw 'Expected replacement block was not found.' }

    $GetItem = ($Instructions | Where-Object Offset -eq 0x0054).Operand
    $CallOpcode = ($Instructions | Where-Object Offset -eq 0x0064).Operand
    $PluginType = ($Instructions | Where-Object Offset -eq 0x0069).Operand
    $GetTypeFromHandle = ($Instructions | Where-Object Offset -eq 0x006E).Operand
    $PropertyGetter = ($Instructions | Where-Object Offset -eq 0x0078).Operand
    $CodeInstructionType = ($Instructions | Where-Object Offset -eq 0x007D).Operand.DeclaringType.Resolve()
    $OpcodeField = $Module.ImportReference(($CodeInstructionType.Fields | Where-Object Name -eq 'opcode'))
    $OperandField = $Module.ImportReference(($CodeInstructionType.Fields | Where-Object Name -eq 'operand'))

    $Processor = $Method.Body.GetILProcessor()
    $New = @(
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Ldloc_0),
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Ldloc_2),
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Ldc_I4_1),
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Sub),
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Callvirt, $GetItem),
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Dup),
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Ldsfld, $CallOpcode),
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Stfld, $OpcodeField),
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Ldtoken, $PluginType),
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Call, $GetTypeFromHandle),
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Ldstr, 'Limit'),
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Call, $PropertyGetter),
        [Mono.Cecil.Cil.Instruction]::Create([Mono.Cecil.Cil.OpCodes]::Stfld, $OperandField)
    )

    foreach ($Instruction in $New) { $Processor.InsertBefore($Start, $Instruction) }
    $Current = $Start
    while ($true) {
        $Next = $Current.Next
        $Processor.Remove($Current)
        if ($Current -eq $End) { break }
        $Current = $Next
    }

    New-Item -ItemType Directory -Force (Split-Path -Parent $OutputDll) | Out-Null
    $Assembly.Write($OutputDll)
}
finally {
    $Assembly.Dispose()
    $Resolver.Dispose()
}

Get-FileHash -LiteralPath $OutputDll -Algorithm SHA256
