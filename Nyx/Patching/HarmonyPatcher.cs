using HarmonyLib;
using System.Reflection;
using Nyx.Core;

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
				harmony.PatchAll(Assembly.GetExecutingAssembly());
				ConsoleLogger.Log(LogType.Info, "[HarmonyPatcher] Patches applied.");
			}
		}
	}
}
