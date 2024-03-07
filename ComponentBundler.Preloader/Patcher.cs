using System;
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
        var pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        while (true) {
            pluginDirectory = Path.Combine(pluginDirectory, "..");
            if (Directory.Exists(Path.Combine(pluginDirectory, "plugins"))) {
                break;
            }
        }
        
        RecursiveSearch<Dictionary<string, string[]>>(pluginDirectory, "bundler_config", file => {
            foreach (var (target, components) in file) {
                foreach (var component in components) {
                    ComponentBundlingPreloader.Bundle(assembly, target, component);
                }
            }
        });
        
        RecursiveSearch<Dictionary<string, string[]>>(pluginDirectory, "method_gen_config", file => {
            foreach (var (target, methods) in file) {
                foreach (var method in methods) {
                    MethodGenerator.CreateMethod(assembly, target, method);
                }
            }
        });
    }

    static void RecursiveSearch<T>(string pluginDirectory, string fileName, Action<T> action) {
        foreach (var subDirectory in Directory.GetDirectories(pluginDirectory)) {
            SearchDirectory(subDirectory, fileName, action);
            foreach (var subSubDirectory in Directory.GetDirectories(subDirectory)) {
                SearchDirectory(subSubDirectory, fileName, action);
            }
        }
    }
    
    static void SearchDirectory<T>(string directory, string fileName, Action<T> action) {
        var files = Directory.GetFiles(directory, fileName + ".json");
        if (files.Length == 0) return;
            
        try {
            var text = File.ReadAllText(files[0]);
            action(JsonConvert.DeserializeObject<T>(text));
        } catch (JsonException e) {
            ComponentBundlingPreloader.Logger.LogError($"Failed to parse config file in {directory}: {e.Message}");
        }
    }
}