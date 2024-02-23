using ComponentBundler.Preloader;
using HarmonyLib;
using UnityEngine;

namespace ComponentBundler;

public static class ComponentBundling {
    public static void AddBundle(Component __instance) {
        var targetType = __instance.GetType();
        if (!ComponentBundlingPreloader.TryGetBundle(targetType.FullName, out var bundle)) {
            Plugin.Log.LogWarning($"No bundle found for {targetType}");
            return;
        }

        foreach (var typeName in bundle) {
            var type = AccessTools.TypeByName(typeName);
            if (type == null) {
                Plugin.Log.LogError($"Type {typeName} not found!");
                continue;
            }
            if (!type.IsSubclassOf(typeof(MonoBehaviour))) {
                Plugin.Log.LogError($"{typeName} is not a MonoBehaviour!");
                continue;
            }
            
            __instance.gameObject.AddComponent(type);
        }
    }
}