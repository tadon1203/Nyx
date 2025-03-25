using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Nyx.Core.Configuration;

public class ModuleConfig
{
	public string ModuleName;
	public bool IsEnabled;
	public int CurrentMode;
	public KeyCode ToggleKey;

	public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();

	public string GetSetting(string key)
	{
		if (Settings.TryGetValue(key, out string value))
		{
			return value;
		}
		return null;
	}

	public T GetSetting<T>(string key, T defaultValue)
	{
		string valueStr = GetSetting(key);
		if (valueStr != null)
		{
			try
			{
				if (typeof(T) == typeof(System.Numerics.Vector4))
				{
					valueStr = valueStr.Trim('(', ')', '<', '>');
					var parts = valueStr.Split(',');
					if (parts.Length == 4 &&
					    float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
					    float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
					    float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float z) &&
					    float.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float w))
					{
						var vector = new System.Numerics.Vector4(x, y, z, w);
						return (T)(object)vector;
					}
					else
					{
						throw new FormatException("Invalid Vector4 format.");
					}
				}
				return (T)Convert.ChangeType(valueStr, typeof(T));
			}
			catch (Exception)
			{
				ConsoleLogger.Log(LogType.Warning, $"Failed to convert the type of setting key '{key}'. Using the default value '{defaultValue}'.");
			}
		}
		return defaultValue;
	}

	public void SetSetting(string key, string value)
	{
		Settings[key] = value;
	}

	public void SetSetting<T>(string key, T value)
	{
		Settings[key] = value.ToString();
	}
}

public class ModuleConfigData
{
	public ModuleConfig[] Modules;
}