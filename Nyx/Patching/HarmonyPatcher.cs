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
                harmony.PatchAll(assembly);
                ConsoleLogger.Log(LogType.Info, "[HarmonyPatcher] All patches applied successfully.");
            }
		}
	}
}
