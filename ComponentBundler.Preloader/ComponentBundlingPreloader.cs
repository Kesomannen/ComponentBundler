using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil;

namespace ComponentBundler.Preloader;

public static class ComponentBundlingPreloader {
    internal static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ComponentBundler.Preloader");
    internal static readonly Dictionary<string, List<string>> BundledComponents = new();

    public static IReadOnlyCollection<string> TargettedComponentFullNames => BundledComponents.Keys;
    
    public static bool Bundle(
        AssemblyDefinition targetAssembly,
        string targetFullName,
        string toAddFullName
    ) {
        if (BundledComponents.TryGetValue(targetFullName, out var bundle)) {
            bundle.Add(toAddFullName);
            Logger.LogInfo($"Bundled {toAddFullName} with {targetFullName}");
            return true;
        }
        
        MethodGenerator.CreateMethod(targetAssembly, targetFullName, "Awake");
        
        BundledComponents.Add(targetFullName, new List<string> { toAddFullName });
        Logger.LogInfo($"Bundled {toAddFullName} with {targetFullName}");
        return true;
    }
    
    public static IEnumerable<string> GetBundle(string targetComponentFullName) {
        return BundledComponents.TryGetValue(targetComponentFullName, out var result) ? result : Enumerable.Empty<string>();
    }
    
    public static bool TryGetBundle(string targetComponentFullName, out IEnumerable<string> bundle) {
        if (BundledComponents.TryGetValue(targetComponentFullName, out var result)) {
            bundle = result;
            return true;
        }
        
        bundle = Enumerable.Empty<string>();
        return false;
    }
}