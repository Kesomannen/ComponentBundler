using System;
using BepInEx;
using BepInEx.Logging;
using ComponentBundler.Preloader;
using HarmonyLib;

namespace ComponentBundler;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource("ComponentBundler");

    void Awake() {
        var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        var prefixMethod = new HarmonyMethod(typeof(ComponentBundling), nameof(ComponentBundling.AddBundle));
            
        foreach (var target in ComponentBundlingPreloader.TargettedComponentFullNames) {
            var type = AccessTools.TypeByName(target);
            if (type == null) {
                Log.LogError($"Type {target} not found!");
                continue;
            }
            
            var awakeMethod = AccessTools.Method(type, ComponentBundlingPreloader.TargetMethodName);
            if (awakeMethod == null) {
                Log.LogError($"Method {target}.Awake not found!");
                continue;
            }

            harmony.Patch(awakeMethod, prefix: prefixMethod);
        }
        
        Log.LogInfo("Plugin loaded!");
    }
}
