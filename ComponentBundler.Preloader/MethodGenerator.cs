using System.Linq;
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace ComponentBundler.Preloader; 

public static class MethodGenerator {
    internal static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("ComponentBundler.MethodGenerator");
    
    public static bool CreateMethod(AssemblyDefinition assembly, string classFullName, string methodName) {
        var targetTypeDefinition = assembly.MainModule.GetType(classFullName);
        if (targetTypeDefinition == null) {
            var message = $"Target type {classFullName} not found in {assembly.Name.Name}!";

            if (!classFullName.Contains('.')) {
                message += " Did you forget to include the namespace?";
            }
            
            Logger.LogError(message);
            return false;
        }
        
        if (targetTypeDefinition.Methods.Any(m => m.Name == methodName)) return true;

        MethodDefinition methodInBaseType = null;
        var baseType = targetTypeDefinition.BaseType;
        while (baseType != null) {
            var baseTypeDefinition = baseType.Resolve();
            if (baseTypeDefinition == null) {
                Logger.LogError($"Failed to resolve base type {baseType.FullName}");
                return false;
            }

            methodInBaseType = baseTypeDefinition.Methods.FirstOrDefault(m => m.Name == methodName);
            if (methodInBaseType != null) break;

            baseType = baseTypeDefinition.BaseType;
        }
        
        var methodDefinition = new MethodDefinition(
            methodName,
            MethodAttributes.Private,
            assembly.MainModule.TypeSystem.Void
        );

        var il = methodDefinition.Body.GetILProcessor();
        if (methodInBaseType != null) {
            il.Emit(OpCodes.Ldarg_0); // this
            il.Emit(OpCodes.Call, methodInBaseType); // .base()
            
            Logger.LogInfo($"Added call to base method {methodInBaseType.Name} in {classFullName}.{methodName}");
        }
        
        il.Emit(OpCodes.Ret);
        
        targetTypeDefinition.Methods.Add(methodDefinition);
        Logger.LogInfo($"Added {methodName} to {classFullName}");

        return true;
    }
}