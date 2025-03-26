using System.Reflection;
using HarmonyLib;
using Nyx.Core;

namespace Nyx.Patching;

public static class HarmonyPatcher
{
	public static void ApplyPatches()
	{
		Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "Nyx.Patching");
		ConsoleLogger.Log(LogType.Info, "[HarmonyPatcher] All patches applied successfully.");
	}
}