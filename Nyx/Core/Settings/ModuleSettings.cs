using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Nyx.Core.Settings;

public class ModuleSettings
{
    public string ModuleName { get; }
    public bool IsEnabled { get; set; }
    public int CurrentMode { get; set; }
    public KeyCode ToggleKey { get; set; }

    public Dictionary<string, string> Settings { get; } = new();

    public ModuleSettings(string moduleName)
    {
        ModuleName = moduleName ?? throw new ArgumentNullException(nameof(moduleName));
    }

    public string GetSetting(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        
        Settings.TryGetValue(key, out string value);
        return value;
    }

    public T GetSetting<T>(string key, T defaultValue)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));

        string valueStr = GetSetting(key);
        if (valueStr == null) return defaultValue;

        try
        {
            if (typeof(T) == typeof(Vector4))
            {
                valueStr = valueStr.Trim('(', ')', '<', '>');
                var parts = valueStr.Split(',');
                if (parts.Length == 4 &&
                    float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float z) &&
                    float.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float w))
                {
                    return (T)(object)new Vector4(x, y, z, w);
                }
                throw new FormatException("Invalid Vector4 format.");
            }

            return (T)Convert.ChangeType(valueStr, typeof(T), CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            ConsoleLogger.Log(LogType.Warning, $"Failed to convert setting '{key}'. Using default. Error: {ex.Message}");
            return defaultValue;
        }
    }

    public void SetSetting(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            ConsoleLogger.Log(LogType.Error, $"Key cannot be null or whitespace {nameof(key)}");
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        }
        
        Settings[key] = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void SetSetting<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            ConsoleLogger.Log(LogType.Error, $"Key cannot be null or whitespace {nameof(key)}");
            throw new ArgumentException("Key cannot be null or whitespace", nameof(key));
        }
        
        Settings[key] = value switch
        {
            SysVec4 vec => $"<{vec.X}, {vec.Y}, {vec.Z}, {vec.W}>",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value?.ToString() ?? string.Empty
        };
    }
}