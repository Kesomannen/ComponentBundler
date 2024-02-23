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
        foreach (var subDirectories in Directory.GetDirectories(pluginsDirectory)) {
            var files = Directory.GetFiles(subDirectories, "bundler_config.json");
            if (files.Length == 0) continue;
            
            try {
                var text = File.ReadAllText(files[0]);
                var json = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(text);
                foreach (var (key, value) in json) {
                    foreach (var v in value) {
                        ComponentBundling.Bundle(assembly, key, v);
                    }
                }
            }
            catch (JsonException e) {
                ComponentBundling.Logger.LogError($"Failed to parse {files[0]}: {e.Message}");
            }
        }
    }

    public static void Finish() {
        
    }
}