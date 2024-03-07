# ComponentBundler

Mod utility for Lethal Company (and any game with BepInEx) which allows you to add methods to the game's classes and easily bundle your own components with built-in ones. The main use case for the former is to add missing unity callback methods, such as `OnDestroy` and `Awake`. 

The main use case of component bundling is to inject your own `NetworkBehaviours`. This is usually difficult because of these reasons:

- `NetworkBehaviours` cannot be added after the corresponding `NetworkObject` has been spawned.
- Spawning usually occurs *after* `Start`, but before `Awake`.
- Most of the game's `MonoBehaviours` *don't* have `Awake` methods to patch into in order to add your own `NetworkBehaviours`.


## Usage

No code is required!

### Adding methods

- Install the mod through [thunderstore](https://thunderstore.io/c/lethal-company/p/Kesomannen/ComponentBundler/) and add it as a dependency for your mod.
- Add a file to `BepInEx/plugins/<MyMod>/` called `method_gen_config.json`.
- The content should be a JSON object, where the keys are **namespaced names** of the classes you want to target. The values should be arrays of methods you want to add. Note that you can only specify the method name: you cannot add arguments or a return type. For example:
```json
{
    "HUDManager": [
        "Awake",
        "OnDestroy"
    ],
    "GameNetcodeStuff.PlayerControllerB": [
        "Awake"
    ]
}
```
- You can now patch the generated methods with Harmony to add custom functionality!


### Bundling components

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
