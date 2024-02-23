using BepInEx;
using BepInEx.Logging;

namespace ComponentBundler;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
    internal static ManualLogSource Log { get; } = BepInEx.Logging.Logger.CreateLogSource("ComponentBundler");
}
