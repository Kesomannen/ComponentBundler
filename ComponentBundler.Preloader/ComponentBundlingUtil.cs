using System;
using UnityEngine;

namespace ComponentBundler.Preloader;

public static class ComponentBundlingUtil {
    public static void AddBundle(Component targetComponent, string targetComponentFullName) {
        if (!ComponentBundling.TryGetBundle(targetComponentFullName, out var bundle)) {
            ComponentBundling.Logger.LogWarning($"No bundle found for {targetComponentFullName}");
            return;
        }

        foreach (var typeName in bundle) {
            var type = Type.GetType(typeName);
            if (type == null) {
                ComponentBundling.Logger.LogError($"Type {typeName} not found!");
                continue;
            }
            if (!type.IsSubclassOf(typeof(MonoBehaviour))) {
                ComponentBundling.Logger.LogError($"{typeName} is not a MonoBehaviour!");
                continue;
            }
            
            targetComponent.gameObject.AddComponent(type);
        }
    }
}