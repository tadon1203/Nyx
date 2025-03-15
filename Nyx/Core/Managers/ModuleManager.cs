using System;
using System.Collections.Generic;
using Nyx.Modules;

namespace Nyx.Core.Managers
{
	public static class ModuleManager
	{
		private static readonly Dictionary<Type, ModuleBase> modules = new()
		{
			{ typeof(Modules.Visual.Menu), new Modules.Visual.Menu() },
			{ typeof(Modules.Movement.Flight), new Modules.Movement.Flight() },
			{ typeof(Modules.Movement.TargetStrafe), new Modules.Movement.TargetStrafe()},
		};

		public static T GetModule<T>() where T : ModuleBase
		{
			modules.TryGetValue(typeof(T), out ModuleBase module);
			return (T)module;
		}

		public static IEnumerable<ModuleBase> GetAllModules() => modules.Values;

		public static void Update()
		{
			foreach (var module in modules.Values)
			{
				module.Update();
			}
		}

		public static void Render()
		{
			foreach (var module in modules.Values)
			{
				module.OnImGuiRender();
			}
		}
	}
}
