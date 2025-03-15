using HarmonyLib;
using System.Reflection;
using Nyx.Core;
using System.Linq;

namespace Nyx.Patching
{
	public static class HarmonyPatcher
	{
		private static Harmony harmony;

		public static void ApplyPatches()
		{
			if (harmony == null)
        {
            harmony = new Harmony("Nyx");
            
            var assembly = Assembly.GetExecutingAssembly();
            var patchMethods = assembly.GetTypes()
                .SelectMany(type => type.GetMethods())
                .Where(method => method.GetCustomAttributes(typeof(HarmonyPatch), false).Length > 0)
                .ToList();
            
            foreach (var method in patchMethods)
            {
                var attributes = method.GetCustomAttributes(typeof(HarmonyPatch), false);
                var targetType = attributes.Length > 0 ? 
                    ((HarmonyPatch)attributes[0]).info.declaringType : null;
                var targetMethod = attributes.Length > 0 ? 
                    ((HarmonyPatch)attributes[0]).info.methodName : "Unknown";
                
                ConsoleLogger.Log(LogType.Info, $"[HarmonyPatcher] Applying patch: {method.DeclaringType.Name}.{method.Name} to {targetType?.Name ?? "Unknown"}.{targetMethod}");
            }
            
            harmony.PatchAll(assembly);
            ConsoleLogger.Log(LogType.Info, "[HarmonyPatcher] All patches applied successfully.");
        }
		}
	}
}
