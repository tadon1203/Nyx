using BepInEx;
using BepInEx.Unity.IL2CPP;
using System;
using Nyx.Core;
using Nyx.Core.Managers;
using Nyx.Patching;
using UnityEngine;

namespace Nyx
{
    [BepInPlugin("Nyx", "Nyx", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            ConsoleLogger.Init();
            HarmonyPatcher.ApplyPatches();
            AddComponent<UnityMainThreadDispatcher>();
            DearImGuiInjection.DearImGuiInjection.Render += ModuleManager.Render;
            DearImGuiInjection.DearImGuiInjection.Render += NotificationManager.Render;
			ConfigManager.LoadConfig();
            AddComponent<MainMonoBehaviour>();
        }
    }

    public class MainMonoBehaviour : MonoBehaviour
    {
        public MainMonoBehaviour(IntPtr handle) : base(handle) { }

		private void Update()
        {
            ModuleManager.Update();
            NotificationManager.Update(Time.deltaTime);
        }
    }
}
