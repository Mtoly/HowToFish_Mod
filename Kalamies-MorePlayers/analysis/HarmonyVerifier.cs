using System;
using System.IO;
using System.Linq;
using System.Reflection;

internal static class HarmonyVerifier
{
    private static readonly string Game = @"E:\SteamLibrary\steamapps\common\How to Fish\How to Fish";
    private static string PluginDirectory = "";

    private static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: HarmonyVerifier PLUGIN_DLL");
            return 2;
        }

        var plugin = Path.GetFullPath(args[0]);
        PluginDirectory = Path.GetDirectoryName(plugin);
        AppDomain.CurrentDomain.AssemblyResolve += Resolve;

        try
        {
            var core = Path.Combine(Game, "BepInEx", "core");
            var managed = Path.Combine(Game, "How to Fish_Data", "Managed");
            var harmonyAssembly = Assembly.LoadFrom(Path.Combine(core, "0Harmony.dll"));
            Assembly.LoadFrom(Path.Combine(core, "BepInEx.dll"));
            Assembly.LoadFrom(Path.Combine(managed, "Assembly-CSharp.dll"));
            var pluginAssembly = Assembly.LoadFrom(plugin);

            var harmonyType = harmonyAssembly.GetType("HarmonyLib.Harmony", true);
            var harmony = Activator.CreateInstance(harmonyType, "moreplayers.verify." + Guid.NewGuid());
            var patchAll = harmonyType.GetMethods().Single(method =>
                method.Name == "PatchAll" &&
                method.GetParameters().Length == 1 &&
                method.GetParameters()[0].ParameterType == typeof(Assembly));
            patchAll.Invoke(harmony, new object[] { pluginAssembly });

            Console.WriteLine("PLUGIN=" + plugin);
            Console.WriteLine("RESULT=PASS_HARMONY_PATCHALL");
            return 0;
        }
        catch (Exception exception)
        {
            while (exception is TargetInvocationException && exception.InnerException != null)
                exception = exception.InnerException;
            Console.WriteLine("PLUGIN=" + plugin);
            Console.WriteLine("RESULT=FAIL_HARMONY_PATCHALL");
            Console.WriteLine("ERROR_TYPE=" + exception.GetType().FullName);
            Console.WriteLine("ERROR_MESSAGE=" + exception.Message);
            Console.WriteLine("ERROR_DETAIL=" + exception);
            return 1;
        }
    }

    private static Assembly Resolve(object sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name).Name + ".dll";
        var candidates = new[]
        {
            Path.Combine(Game, "BepInEx", "core", name),
            Path.Combine(Game, "How to Fish_Data", "Managed", name),
            Path.Combine(PluginDirectory, name)
        };
        var match = candidates.FirstOrDefault(File.Exists);
        return match == null ? null : Assembly.LoadFrom(match);
    }
}
