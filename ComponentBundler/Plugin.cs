using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using ComponentBundler.Preloader;
using HarmonyLib;
using UnityEngine;

namespace ComponentBundler;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource("ComponentBundler");

    static readonly Dictionary<Type, Type[]> _loadedBundles = new();
    
    void Awake() {
        var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        var prefixMethod = new HarmonyMethod(typeof(Plugin), nameof(Awake_Prefix));
            
        foreach (var target in ComponentBundlingPreloader.TargettedComponentFullNames) {
            var targetType = AccessTools.TypeByName(target)!; // this is already validated in ComponentBundlingPreloader
            
            var awakeMethod = AccessTools.Method(targetType, ComponentBundlingPreloader.TargetMethodName);
            if (awakeMethod == null) {
                Log.LogError($"Method {target}.Awake not found, please report this to the mod author");
                continue;
            }
            
            if (!ComponentBundlingPreloader.TryGetBundle(target, out var bundle)) {
                Log.LogWarning($"No bundle found for {target}. Skipping...");
                continue;
            }

            _loadedBundles[targetType] = bundle.Select(ValidateBundledType).Where(t => t != null).ToArray();
            harmony.Patch(awakeMethod, prefix: prefixMethod);
        }
        
        Log.LogInfo("Plugin loaded!");
    }

    static Type ValidateBundledType(string fullName) {
        var type = AccessTools.TypeByName(fullName);
        if (type == null)
        {
            Log.LogError($"Bundled type {fullName} not found!");
            return null;
        }
        if (!type.IsSubclassOf(typeof(MonoBehaviour)))
        {
            Log.LogError($"Bundled type {type} is not a MonoBehaviour!");
            return null;
        }
        return type;
    }

    static void Awake_Prefix(Component __instance) {
        foreach (var type in _loadedBundles[__instance.GetType()]) {
            __instance.gameObject.AddComponent(type);
        }
    }
}
