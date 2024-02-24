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
        var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        while (true) {
            currentDirectory = Path.Combine(currentDirectory, "..");
            if (Directory.Exists(Path.Combine(currentDirectory, "plugins")))
                break;
        }
        
        foreach (var subDirectory in Directory.GetDirectories(currentDirectory)) {
            SearchDirectory(subDirectory);
            foreach (var subSubDirectory in Directory.GetDirectories(subDirectory)) {
                SearchDirectory(subSubDirectory);
            }
        }
        return;

        void SearchDirectory(string directory) {
            var files = Directory.GetFiles(directory, "bundler_config.json");
            if (files.Length == 0) return;
            
            try {
                var text = File.ReadAllText(files[0]);
                var json = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(text);
                foreach (var (target, components) in json) {
                    foreach (var component in components) {
                        ComponentBundlingPreloader.Bundle(assembly, target, component);
                    }
                }
            } catch (JsonException e) {
                ComponentBundlingPreloader.Logger.LogError($"Failed to parse config file in {directory}: {e.Message}");
            }
        }
    }
}