using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nyx.Core.Configuration
{
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
					return (T)Convert.ChangeType(valueStr, typeof(T));
				}
				catch (Exception)
				{
					ConsoleLogger.Log(LogType.Warning, $"設定キー '{key}' の型変換に失敗しました。デフォルト値 '{defaultValue}' を使用します。");
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
}
