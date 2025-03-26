using System;

namespace Nyx.Core.Settings;

[AttributeUsage(AttributeTargets.Field)]
public class SettingAttribute : Attribute
{
    public string DisplayName { get; }
    public string Description { get; }
    public string DefaultValue { get; }
    public Type ValueType { get; }

    public SettingAttribute(string displayName, string description, string defaultValue, Type valueType = null)
    {
        DisplayName = displayName;
        Description = description;
        DefaultValue = defaultValue;
        ValueType = valueType ?? typeof(string);
    }
}
