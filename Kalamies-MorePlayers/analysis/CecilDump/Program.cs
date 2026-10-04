using Mono.Cecil;
using Mono.Cecil.Cil;

if (args.Length != 3)
{
    Console.Error.WriteLine("Usage: CecilDump ASSEMBLY TYPE_OR_METHOD_FILTER OUTPUT");
    return 2;
}

var assemblyPath = Path.GetFullPath(args[0]);
var filter = args[1];
var outputPath = Path.GetFullPath(args[2]);

var resolver = new DefaultAssemblyResolver();
resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath)!);
using var assembly = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters
{
    ReadSymbols = false,
    ReadingMode = ReadingMode.Immediate,
    AssemblyResolver = resolver
});
var types = Flatten(assembly.MainModule.Types).ToArray();

var matches = types.Where(type => Matches(type, filter)).ToArray();
await using var stream = File.CreateText(outputPath);
await stream.WriteLineAsync($"Assembly: {assembly.Name.FullName}");
await stream.WriteLineAsync($"Path: {assemblyPath}");
await stream.WriteLineAsync($"Filter: {filter}");
await stream.WriteLineAsync($"Matched types: {matches.Length}");

foreach (var type in matches)
{
    await stream.WriteLineAsync();
    await stream.WriteLineAsync($"TYPE {type.FullName}");
    foreach (var attribute in type.CustomAttributes)
        await stream.WriteLineAsync($"  ATTRIBUTE {FormatAttribute(attribute)}");

    foreach (var field in type.Fields)
        await stream.WriteLineAsync($"  FIELD {field.Attributes} {field.FieldType.FullName} {field.Name}");

    foreach (var method in type.Methods.Where(method => MethodMatches(type, method, filter)))
    {
        await stream.WriteLineAsync();
        await stream.WriteLineAsync($"  METHOD {method.FullName}");
        await stream.WriteLineAsync($"    ATTRIBUTES {method.Attributes}");
        foreach (var attribute in method.CustomAttributes)
            await stream.WriteLineAsync($"    ATTRIBUTE {FormatAttribute(attribute)}");

        if (!method.HasBody)
        {
            await stream.WriteLineAsync("    <no body>");
            continue;
        }

        await stream.WriteLineAsync($"    LOCALS init={method.Body.InitLocals} maxstack={method.Body.MaxStackSize}");
        foreach (var variable in method.Body.Variables)
            await stream.WriteLineAsync($"      V_{variable.Index}: {variable.VariableType.FullName}");

        await stream.WriteLineAsync("    IL");
        foreach (var instruction in method.Body.Instructions)
            await stream.WriteLineAsync($"      IL_{instruction.Offset:X4}: {instruction.OpCode,-12} {FormatOperand(instruction.Operand)}".TrimEnd());

        foreach (var handler in method.Body.ExceptionHandlers)
            await stream.WriteLineAsync($"    HANDLER {handler.HandlerType} try=IL_{handler.TryStart?.Offset:X4}..IL_{handler.TryEnd?.Offset:X4} handler=IL_{handler.HandlerStart?.Offset:X4}..IL_{handler.HandlerEnd?.Offset:X4}");
    }
}

return matches.Length == 0 ? 3 : 0;

static IEnumerable<TypeDefinition> Flatten(IEnumerable<TypeDefinition> roots)
{
    foreach (var type in roots)
    {
        yield return type;
        foreach (var nested in Flatten(type.NestedTypes))
            yield return nested;
    }
}

static bool Matches(TypeDefinition type, string filter) =>
    type.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
    type.Methods.Any(method => method.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
    (filter.Equals("plugin", StringComparison.OrdinalIgnoreCase) &&
     (type.CustomAttributes.Any(attribute => attribute.AttributeType.FullName.Contains("Harmony", StringComparison.OrdinalIgnoreCase)) ||
      type.Methods.Any(method => method.Name.Contains("Lobby", StringComparison.OrdinalIgnoreCase))));

static bool MethodMatches(TypeDefinition type, MethodDefinition method, string filter) =>
    type.FullName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
    method.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
    filter.Equals("plugin", StringComparison.OrdinalIgnoreCase);

static string FormatOperand(object? operand) => operand switch
{
    null => string.Empty,
    Instruction instruction => $"IL_{instruction.Offset:X4}",
    Instruction[] instructions => string.Join(", ", instructions.Select(item => $"IL_{item.Offset:X4}")),
    MethodReference method => method.FullName,
    FieldReference field => field.FullName,
    TypeReference type => type.FullName,
    ParameterDefinition parameter => $"arg:{parameter.Index}:{parameter.Name}",
    VariableDefinition variable => $"V_{variable.Index}",
    string text => $"\\\"{text.Replace("\\r", "\\\\r").Replace("\\n", "\\\\n")}\\\"",
    _ => operand.ToString() ?? string.Empty
};

static string FormatAttribute(CustomAttribute attribute)
{
    var constructor = string.Join(", ", attribute.ConstructorArguments.Select(argument => FormatOperand(argument.Value)));
    var properties = string.Join(", ", attribute.Properties.Select(property => $"{property.Name}={FormatOperand(property.Argument.Value)}"));
    return $"{attribute.AttributeType.FullName}({constructor}){(properties.Length == 0 ? string.Empty : " { " + properties + " }")}";
}


