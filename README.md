# ComponentBundler

Utility mod for Lethal Company (and any game with BepInEx) which allows you to bundle your own components with built-in ones. 
The main use case of this mod is to add a `NetworkBehaviour` before the `NetworkObject` is spawned.
The reason we can't just patch a script on the target `NetworkObject` to do this is because of two reasons:

- `NetworkObject` spawning occurs after `Awake` but *before* `Start`.
- Most `MonoBehaviours` in the game *don't* have an Awake method, and it cannot be added through runtime patching (Harmony).

This mod works by running a preload patcher which adds `Awake` methods where it is needed.

## Usage

No code is required! 

- Install the mod through [thunderstore](https://thunderstore.io/c/lethal-company/p/Kesomannen/ComponentBundler/) and add it as a dependency for your mod.
- Add a file to `BepInEx/plugins/<MyMod>/` called `bundler_config.json`.
- The content should be a JSON object, where the keys are **namespaced names** of the game components you want to target. The values should be arrays of **namespaced names** of your components that you want to bundle. For example:
```json
{
    "FlashlightItem": [
        "MyMod.FlashlightAddition"
    ],
    "GameNetcodeStuff.PlayerControllerB": [
        "MyMod.Player.PlayerComponent1",
        "MyMod.Player.PlayerComponent2"
    ]
}
```
