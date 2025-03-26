using System;
using System.Globalization;

namespace Nyx.Core.Settings;

public static class SettingSerializer
{
    public static string Serialize(object value)
    {
        if (value == null) return null;

        switch (value)
        {
            case SysVec2 vec2:
                return $"{vec2.X},{vec2.Y}";
            case SysVec3 vec3:
                return $"{vec3.X},{vec3.Y},{vec3.Z}";
            case SysVec4 vec4:
                return $"{vec4.X},{vec4.Y},{vec4.Z},{vec4.W}";
            default:
                return value.ToString();
        }
    }

    public static object Deserialize(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value)) 
            return GetDefaultValue(targetType);

        try
        {
            if (targetType == typeof(SysVec2))
            {
                var parts = value.Split(',');
                if (parts.Length != 2) throw new FormatException("Invalid SysVec2 format");
                return new SysVec2(
                    float.Parse(parts[0], CultureInfo.InvariantCulture),
                    float.Parse(parts[1], CultureInfo.InvariantCulture));
            }

            if (targetType == typeof(SysVec3))
            {
                var parts = value.Split(',');
                if (parts.Length != 3) throw new FormatException("Invalid SysVec3 format");
                return new SysVec3(
                    float.Parse(parts[0], CultureInfo.InvariantCulture),
                    float.Parse(parts[1], CultureInfo.InvariantCulture),
                    float.Parse(parts[2], CultureInfo.InvariantCulture));
            }

            if (targetType == typeof(SysVec4))
            {
                var parts = value.Split(',');
                if (parts.Length != 4) throw new FormatException("Invalid SysVec4 format");
                return new SysVec4(
                    float.Parse(parts[0], CultureInfo.InvariantCulture),
                    float.Parse(parts[1], CultureInfo.InvariantCulture),
                    float.Parse(parts[2], CultureInfo.InvariantCulture),
                    float.Parse(parts[3], CultureInfo.InvariantCulture));
            }
            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value);
            }

            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            ConsoleLogger.Log(LogType.Warning, 
                $"Failed to deserialize value '{value}' to type {targetType.Name}. Error: {ex.Message}");
            return GetDefaultValue(targetType);
        }
    }
    
    private static object GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}
