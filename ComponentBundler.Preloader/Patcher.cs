using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using Newtonsoft.Json;

namespace ComponentBundler.Preloader;

public static class Patcher {
    public static IEnumerable<string> TargetDLLs {
        get {
            yield return "Assembly-CSharp.dll";
        }
    }

    public static void Patch(AssemblyDefinition assembly) {
        var preloaderDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var pluginsDirectory = Path.Combine(preloaderDirectory, "..", "plugins");
        
        foreach (var subDirectory in Directory.GetDirectories(pluginsDirectory)) {
            var files = Directory.GetFiles(subDirectory, "bundler_config.json");
            if (files.Length == 0) continue;
            
            try {
                var text = File.ReadAllText(files[0]);
                var json = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(text);
                foreach (var (target, components) in json) {
                    foreach (var component in components) {
                        ComponentBundlingPreloader.Bundle(assembly, target, component);
                    }
                }
            } catch (JsonException e) {
                ComponentBundlingPreloader.Logger.LogError($"Failed to parse config file in {subDirectory}: {e.Message}");
            }
        }
    }
}