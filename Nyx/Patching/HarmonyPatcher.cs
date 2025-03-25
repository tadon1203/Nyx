using HarmonyLib;
using System.Reflection;
using Nyx.Core;

namespace Nyx.Patching;

public static class HarmonyPatcher
{
	private static Harmony _harmony;

	public static void ApplyPatches()
	{
		if (_harmony == null)
		{
			_harmony = new Harmony("Nyx");
                
			var assembly = Assembly.GetExecutingAssembly();              
			_harmony.PatchAll(assembly);
			ConsoleLogger.Log(LogType.Info, "[HarmonyPatcher] All patches applied successfully.");
		}
	}
}