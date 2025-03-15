using Nyx.Core.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Nyx.Core.Managers
{
	public static class ConfigManager
	{
		private static readonly string configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Nyx", "Config.json");
		private const string LOG_PREFIX = "[ConfigManager]";

		public static void SaveConfig()
		{
			try
			{
				var configs = ModuleManager.GetAllModules().Select(module =>
				{
					var config = new ModuleConfig
					{
						ModuleName = module.Name,
						IsEnabled = module.IsEnabled,
						CurrentMode = module.CurrentMode,
						ToggleKey = module.ToggleKey
					};
					if (module is IConfigurableModule configurableModule)
					{
						configurableModule.SaveModuleConfig(config);
					}
					return config;
				}).ToArray();

				var data = new ModuleConfigData { Modules = configs };
				var options = new JsonSerializerOptions
				{
					WriteIndented = true,
					IncludeFields = true
				};
				string json = JsonSerializer.Serialize(data, options);

				string folder = Path.GetDirectoryName(configFilePath);
				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
					ConsoleLogger.Log(LogType.Debug, $"{LOG_PREFIX} Created directory: {folder}");
				}

				File.WriteAllText(configFilePath, json);
				ConsoleLogger.Log(LogType.Info, $"{LOG_PREFIX} Configuration saved to: {configFilePath}");
			}
			catch (Exception ex)
			{
				ConsoleLogger.Log(LogType.Error, $"{LOG_PREFIX} Failed to save configuration: {ex.Message}");
			}
		}

		public static void LoadConfig()
		{
			try
			{
				if (!File.Exists(configFilePath))
				{
					ConsoleLogger.Log(LogType.Warning, $"{LOG_PREFIX} Configuration file not found: {configFilePath}");
					ConsoleLogger.Log(LogType.Info, $"{LOG_PREFIX} Creating default configuration...");
					SaveConfig();
					return;
				}

				string json = File.ReadAllText(configFilePath);
				var options = new JsonSerializerOptions
				{
					IncludeFields = true
				};

				ModuleConfigData data = JsonSerializer.Deserialize<ModuleConfigData>(json, options);

				if (data.Modules == null || data.Modules.Length == 0)
				{
					ConsoleLogger.Log(LogType.Warning, $"{LOG_PREFIX} Configuration data is empty or invalid");
					return;
				}

				int loadedModules = 0;
				int failedModules = 0;

				foreach (var config in data.Modules)
				{
					var module = ModuleManager.GetAllModules().FirstOrDefault(m => m.Name == config.ModuleName);
					if (module != null)
					{
						try
						{
							if (module.Name != "Menu")
							{
								if (config.IsEnabled && !module.IsEnabled)
									module.Enable();
								else if (!config.IsEnabled && module.IsEnabled)
									module.Disable();
							}

							if (config.CurrentMode != -1)
							{
								module.SetMode(config.CurrentMode);
							}

							module.SetToggleKey(config.ToggleKey);

							if (module is IConfigurableModule configurableModule)
							{
								configurableModule.LoadModuleConfig(config);
							}

							loadedModules++;
							ConsoleLogger.Log(LogType.Debug, $"{LOG_PREFIX} Module configuration loaded: {module.Name}");
						}
						catch (Exception ex)
						{
							failedModules++;
							ConsoleLogger.Log(LogType.Warning, $"{LOG_PREFIX} Error applying configuration to module '{module.Name}': {ex.Message}");
						}
					}
				}
				ConsoleLogger.Log(LogType.Info, $"Configuration has been loaded: {configFilePath}");
			}
			catch (Exception ex)
			{
				ConsoleLogger.Log(LogType.Error, $"{LOG_PREFIX} Failed to load configuration: {ex.Message}");
			}
		}
	}
}
