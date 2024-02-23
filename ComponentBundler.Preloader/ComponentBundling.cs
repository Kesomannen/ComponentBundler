using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil;
using UnityEngine;
using MethodAttributes = Mono.Cecil.MethodAttributes;

namespace ComponentBundler.Preloader;

public static class ComponentBundling {
    internal static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ComponentBundler.Preloader");
    internal static readonly Dictionary<string, List<string>> BundledComponents = new();

    const string TargetMethodName = "Awake";
    
    public static bool Bundle(
        AssemblyDefinition targetAssembly,
        string targetFullName,
        string toAddAssemblyQualifiedName
    ) {
        if (BundledComponents.TryGetValue(targetFullName, out var bundle)) {
            bundle.Add(toAddAssemblyQualifiedName);
            Logger.LogInfo($"Bundled {toAddAssemblyQualifiedName} with {targetFullName}");
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
                MethodAttributes.Public,
                targetAssembly.MainModule.TypeSystem.Void
            );

            targetTypeDefinition.Methods.Add(methodDefinition);
        }
        
        BundledComponents.Add(targetFullName, new List<string> { toAddAssemblyQualifiedName });
        Logger.LogInfo($"Bundled {toAddAssemblyQualifiedName} with {targetFullName}");
        return true;
    }
    
    public static bool TryGetBundle(string targetComponentFullName, out IReadOnlyList<string> bundle) {
        if (!BundledComponents.TryGetValue(targetComponentFullName, out var result)) {
            bundle = null;
            return false;
        }
        
        bundle = result;
        return true;
    }
}