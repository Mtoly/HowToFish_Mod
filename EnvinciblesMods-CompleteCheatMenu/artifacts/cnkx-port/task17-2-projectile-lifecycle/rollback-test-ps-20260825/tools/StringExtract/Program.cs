using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;

var path = args.Length > 0 ? args[0] : "CompleteCheatMenu.dll";
using var fs = File.OpenRead(path);
using var pe = new PEReader(fs);
var md = pe.GetMetadataReader();
var seen = new HashSet<string>(StringComparer.Ordinal);
foreach (var h in md.UserStrings) {
    var s = md.GetUserString(h);
    if (s.Length >= 2 && s.Any(char.IsLetter) && seen.Add(s)) Console.WriteLine(s.Replace("\r", "\\r").Replace("\n", "\\n"));
}
Console.Error.WriteLine($"UserStrings={seen.Count}; Assembly={md.GetAssemblyDefinition().Name}");
