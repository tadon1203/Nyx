using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Nyx.Core.Settings;

namespace Nyx.Core.Managers;

public static class SettingsManager
{
    private static readonly string SettingsFilePath = Path.Combine(
        Directory.GetCurrentDirectory(), "Nyx", "Settings.json");
    
    private const string LogPrefix = "[SettingsManager]";

    public static void SaveSettings()
    {
        try
        {
            var settings = ModuleManager.GetAllModules().Select(module =>
            {
                var moduleSettings = new ModuleSettings(module.Name)
                {
                    IsEnabled = module.IsEnabled,
                    CurrentMode = module.CurrentMode,
                    ToggleKey = module.ToggleKey
                };
                
                module.SaveSettings(moduleSettings);
                return moduleSettings;
            }).ToArray();

            var data = new ModuleSettingsData { Modules = settings };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };

            string directory = Path.GetDirectoryName(SettingsFilePath)!;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                ConsoleLogger.Log(LogType.Debug, $"{LogPrefix} Created directory: {directory}");
            }

            File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(data, options));
            ConsoleLogger.Log(LogType.Info, $"{LogPrefix} Settings saved to: {SettingsFilePath}");
            NotificationManager.AddNotification("SettingsManager", "Settings saved to: " + SettingsFilePath);
        }
        catch (Exception ex)
        {
            ConsoleLogger.Log(LogType.Error, $"{LogPrefix} Failed to save settings: {ex.Message}");
        }
    }

    public static void LoadSettings()
    {
        try
        {
            if (!File.Exists(SettingsFilePath))
            {
                ConsoleLogger.Log(LogType.Warning, $"{LogPrefix} Settings file not found. Creating default...");
                SaveSettings();
                return;
            }

            var data = JsonSerializer.Deserialize<ModuleSettingsData>(
                File.ReadAllText(SettingsFilePath));

            if (data?.Modules == null || data.Modules.Length == 0)
            {
                ConsoleLogger.Log(LogType.Warning, $"{LogPrefix} No valid settings found");
                return;
            }

            foreach (var moduleSettings in data.Modules)
            {
                var module = ModuleManager.GetModule(moduleSettings.ModuleName);
                if (module == null) continue;

                if (module.Name != "Menu")
                {
                    if (moduleSettings.IsEnabled) module.Enable();
                    else module.Disable();
                }

                module.SetMode(moduleSettings.CurrentMode);
                module.SetToggleKey(moduleSettings.ToggleKey);
                module.LoadSettings(moduleSettings);
            }
        }
        catch (Exception ex)
        {
            ConsoleLogger.Log(LogType.Error, $"{LogPrefix} Failed to load settings: {ex.Message}");
        }
    }
}