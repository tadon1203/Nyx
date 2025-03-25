using System;
using System.Collections.Generic;
using Nyx.Modules;

namespace Nyx.Core.Managers;

public static class ModuleManager
{
	private static readonly Dictionary<Type, ModuleBase> Modules = new()
	{
		{ typeof(Modules.Visual.Menu), new Modules.Visual.Menu() },
		{ typeof(Modules.Movement.Flight), new Modules.Movement.Flight() },
		{ typeof(Modules.Movement.TargetStrafe), new Modules.Movement.TargetStrafe()},
		{ typeof(Modules.Visual.TwoDimensionalESP), new Modules.Visual.TwoDimensionalESP() },
		{ typeof(Modules.Visual.ThreeDimensionalESP), new Modules.Visual.ThreeDimensionalESP() },
		{ typeof(Modules.Visual.BoneESP), new Modules.Visual.BoneESP() },
	};

	public static T GetModule<T>() where T : ModuleBase
	{
		Modules.TryGetValue(typeof(T), out ModuleBase module);
		return (T)module;
	}

	public static IEnumerable<ModuleBase> GetAllModules() => Modules.Values;

	public static void Update()
	{
		foreach (var module in Modules.Values)
		{
			module.Update();
		}
	}

	public static void Render()
	{
		foreach (var module in Modules.Values)
		{
			module.OnImGuiRender();
		}
	}
}