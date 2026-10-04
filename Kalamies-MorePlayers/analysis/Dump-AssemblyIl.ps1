param(
    [Parameter(Mandatory)] [string] $AssemblyPath,
    [Parameter(Mandatory)] [string] $Filter,
    [Parameter(Mandatory)] [string] $OutputPath
)

$ErrorActionPreference = 'Stop'
$Cecil = 'E:\SteamLibrary\steamapps\common\How to Fish\How to Fish\BepInEx\core\Mono.Cecil.dll'
Add-Type -LiteralPath $Cecil

function Get-AllTypes($Types) {
    foreach ($Type in $Types) {
        $Type
        Get-AllTypes $Type.NestedTypes
    }
}

function Format-Operand($Operand) {
    if ($null -eq $Operand) { return '' }
    if ($Operand -is [Mono.Cecil.Cil.Instruction]) { return ('IL_{0:X4}' -f $Operand.Offset) }
    if ($Operand -is [System.Array] -and $Operand.Count -gt 0 -and $Operand[0] -is [Mono.Cecil.Cil.Instruction]) {
        return (($Operand | ForEach-Object { 'IL_{0:X4}' -f $_.Offset }) -join ', ')
    }
    if ($Operand -is [Mono.Cecil.MethodReference] -or
        $Operand -is [Mono.Cecil.FieldReference] -or
        $Operand -is [Mono.Cecil.TypeReference]) { return $Operand.FullName }
    if ($Operand -is [Mono.Cecil.ParameterDefinition]) { return "arg:$($Operand.Index):$($Operand.Name)" }
    if ($Operand -is [Mono.Cecil.Cil.VariableDefinition]) { return "V_$($Operand.Index)" }
    if ($Operand -is [string]) { return '"' + $Operand.Replace("`r", '\r').Replace("`n", '\n') + '"' }
    return [string]$Operand
}

function Format-Attribute($Attribute) {
    $Arguments = ($Attribute.ConstructorArguments | ForEach-Object { Format-Operand $_.Value }) -join ', '
    return "$($Attribute.AttributeType.FullName)($Arguments)"
}

$Assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Resolve-Path -LiteralPath $AssemblyPath).Path)
try {
    $Types = @(Get-AllTypes $Assembly.MainModule.Types)
    $Matches = @($Types | Where-Object {
        $_.FullName.Contains($Filter, [StringComparison]::OrdinalIgnoreCase) -or
        @($_.Methods | Where-Object { $_.Name.Contains($Filter, [StringComparison]::OrdinalIgnoreCase) }).Count -gt 0 -or
        ($Filter -eq 'plugin' -and (
            @($_.CustomAttributes | Where-Object { $_.AttributeType.FullName -match 'Harmony' }).Count -gt 0 -or
            @($_.Methods | Where-Object { $_.Name -match 'Lobby|Patch|Transpiler|Prefix|Postfix' }).Count -gt 0
        ))
    })

    $Lines = [Collections.Generic.List[string]]::new()
    $Lines.Add("Assembly: $($Assembly.Name.FullName)")
    $Lines.Add("Path: $((Resolve-Path -LiteralPath $AssemblyPath).Path)")
    $Lines.Add("Filter: $Filter")
    $Lines.Add("Matched types: $($Matches.Count)")

    foreach ($Type in $Matches) {
        $Lines.Add('')
        $Lines.Add("TYPE $($Type.FullName)")
        foreach ($Attribute in $Type.CustomAttributes) { $Lines.Add("  ATTRIBUTE $(Format-Attribute $Attribute)") }
        foreach ($Field in $Type.Fields) { $Lines.Add("  FIELD $($Field.Attributes) $($Field.FieldType.FullName) $($Field.Name)") }

        foreach ($Method in $Type.Methods) {
            if ($Filter -ne 'plugin' -and
                -not $Type.FullName.Contains($Filter, [StringComparison]::OrdinalIgnoreCase) -and
                -not $Method.Name.Contains($Filter, [StringComparison]::OrdinalIgnoreCase)) { continue }

            $Lines.Add('')
            $Lines.Add("  METHOD $($Method.FullName)")
            $Lines.Add("    ATTRIBUTES $($Method.Attributes)")
            foreach ($Attribute in $Method.CustomAttributes) { $Lines.Add("    ATTRIBUTE $(Format-Attribute $Attribute)") }
            if (-not $Method.HasBody) { $Lines.Add('    <no body>'); continue }

            $Lines.Add("    LOCALS init=$($Method.Body.InitLocals) maxstack=$($Method.Body.MaxStackSize)")
            foreach ($Variable in $Method.Body.Variables) { $Lines.Add("      V_$($Variable.Index): $($Variable.VariableType.FullName)") }
            $Lines.Add('    IL')
            foreach ($Instruction in $Method.Body.Instructions) {
                $Lines.Add(('      IL_{0:X4}: {1,-12} {2}' -f $Instruction.Offset, $Instruction.OpCode, (Format-Operand $Instruction.Operand)).TrimEnd())
            }
        }
    }

    $OutputDirectory = Split-Path -Parent $OutputPath
    New-Item -ItemType Directory -Force $OutputDirectory | Out-Null
    $Lines | Set-Content -LiteralPath $OutputPath -Encoding UTF8
    Write-Output "MATCHED_TYPES=$($Matches.Count) OUTPUT=$OutputPath"
    if ($Matches.Count -eq 0) { exit 3 }
}
finally {
    $Assembly.Dispose()
}
