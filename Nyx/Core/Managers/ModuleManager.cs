using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Nyx.Core.Settings;
using Nyx.Modules;

namespace Nyx.Core.Managers;

public static class ModuleManager
{
    private static readonly Dictionary<Type, ModuleBase> ModulesByType = new();
    private static readonly Dictionary<string, ModuleBase> ModulesByName = new();

    static ModuleManager()
    {
        RegisterAllModules();
    }

    private static void RegisterAllModules()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        foreach (Type type in assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(ModuleBase)) && !t.IsAbstract))
        {
            if (Activator.CreateInstance(type) is ModuleBase module)
            {
                ModulesByType[type] = module;
                ModulesByName[module.Name] = module;
            }
        }
    }

    public static T GetModule<T>() where T : ModuleBase
    {
        return ModulesByType.TryGetValue(typeof(T), out var module) 
            ? (T)module 
            : throw new KeyNotFoundException($"Module of type {typeof(T)} not found.");
    }

    public static ModuleBase GetModule(string moduleName)
    {
        ModulesByName.TryGetValue(moduleName, out var module);
        return module;
    }

    public static IEnumerable<ModuleBase> GetAllModules() => ModulesByType.Values;

    public static void Update()
    {
        foreach (var module in ModulesByType.Values)
        {
            module.Update();
        }
    }

    public static void Render()
    {
        foreach (var module in ModulesByType.Values.Where(m => m.IsEnabled))
        {
            module.OnImGuiRender();
        }
    }

    public static void SaveAllSettings()
    {
        var settingsData = new ModuleSettingsData
        {
            Modules = ModulesByType.Values.Select(module =>
            {
                var settings = new ModuleSettings(module.Name)
                {
                    IsEnabled = module.IsEnabled,
                    CurrentMode = module.CurrentMode,
                    ToggleKey = module.ToggleKey
                };
                module.SaveSettings(settings);
                return settings;
            }).ToArray()
        };

        string json = JsonSerializer.Serialize(settingsData, new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true
        });

        File.WriteAllText("NyxSettings.json", json);
    }

    public static void LoadAllSettings()
    {
        if (!File.Exists("NyxSettings.json")) return;
        
        var settingsData = JsonSerializer.Deserialize<ModuleSettingsData>(
            File.ReadAllText("NyxSettings.json"));

        foreach (var moduleSettings in settingsData?.Modules ?? Array.Empty<ModuleSettings>())
        {
            var module = GetModule(moduleSettings.ModuleName);
            if (module == null) continue;

            if (moduleSettings.IsEnabled) module.Enable();
            else module.Disable();

            module.SetMode(moduleSettings.CurrentMode);
            module.SetToggleKey(moduleSettings.ToggleKey);
            module.LoadSettings(moduleSettings);
        }
    }
}
