using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace ComponentBundler.Preloader;

public static class ComponentBundlingPreloader {
    internal static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ComponentBundler.Preloader");
    internal static readonly Dictionary<string, List<string>> BundledComponents = new();

    public const string TargetMethodName = "Awake";

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

        var targetTypeDefinition = targetAssembly.MainModule.GetType(targetFullName);
        if (targetTypeDefinition == null) {
            var message = $"Type {targetFullName} not found in assembly {targetAssembly.Name.Name}.";

            if (!targetFullName.Contains('.')) {
                message += " Did you forget to include the namespace?";
            }
            
            Logger.LogError(message);
            return false;
        }
        
        if (targetTypeDefinition.Methods.All(m => m.Name != TargetMethodName)) {
            var methodDefinition = new MethodDefinition(
                TargetMethodName,
                MethodAttributes.Private | MethodAttributes.HideBySig,
                targetAssembly.MainModule.TypeSystem.Void
            );

            targetTypeDefinition.Methods.Add(methodDefinition);
            methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            Logger.LogInfo($"Added {TargetMethodName} to {targetFullName}");
        }
        
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