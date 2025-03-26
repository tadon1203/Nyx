using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ImGuiNET;
using Nyx.Core;
using Nyx.Core.Managers;
using Nyx.Core.Settings;
using UnityEngine;
using VRC.SDKBase;
using LogType = Nyx.Core.LogType;

namespace Nyx.Modules;

public abstract class ModuleBase
{
    public string Name { get; }
    public string Description { get; }
    public ModuleCategory Category { get; }
    public KeyCode ToggleKey { get; private set; }
    public bool IsEnabled { get; private set; }
    public virtual IReadOnlyList<string> AvailableModes { get; } = Array.Empty<string>();
    public int CurrentMode { get; private set; } = -1;
    
    private Dictionary<string, (FieldInfo Field, SettingAttribute Attr)> _settingFields;

    protected ModuleBase(
        string name,
        string description,
        ModuleCategory category,
        KeyCode toggleKey = KeyCode.None,
        bool isEnabled = false)
    {
        Name = name;
        Description = description;
        Category = category;
        ToggleKey = toggleKey;
        IsEnabled = isEnabled;
    }

    public virtual void OnEnable() { }
    public virtual void OnDisable() { }
    public virtual void OnModeChanged(int newModeIndex) { }
    public virtual void OnUpdate() { }
    public virtual void OnImGuiRender() { }

    public void Enable()
    {
        if (IsEnabled) return;
        
        IsEnabled = true;
        NotificationManager.AddNotification("Module", $"Enabled {Name}.");
        OnEnable();
    }

    public void Disable()
    {
        if (!IsEnabled) return;
        
        IsEnabled = false;
        NotificationManager.AddNotification("Module", $"Disabled {Name}.");
        OnDisable();
    }

    public void Toggle()
    {
        if (IsEnabled)
            Disable();
        else
            Enable();
    }

    public void SetMode(int modeIndex)
    {
        if (AvailableModes.Count == 0)
        {
            CurrentMode = -1;
            return;
        }

        if (modeIndex < -1 || modeIndex >= AvailableModes.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(modeIndex), 
                $"ModeIndex {modeIndex} is not available on module '{Name}'.");
        }

        if (CurrentMode == modeIndex) return;

        CurrentMode = modeIndex;
        if (modeIndex != -1)
        {
            OnModeChanged(modeIndex);
        }
    }

    public void SetToggleKey(KeyCode key) => ToggleKey = key;

    public void Update()
    {
        if (Networking.LocalPlayer != null && IsEnabled)
        {
            OnUpdate();
        }

        if (ToggleKey != KeyCode.None && Input.GetKeyDown(ToggleKey))
        {
            Toggle();
        }
    }
    
    protected void RegisterSettings()
    {
        _settingFields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.GetCustomAttribute<SettingAttribute>() != null)
            .ToDictionary(
                f => f.Name,
                f => (f, f.GetCustomAttribute<SettingAttribute>())
            );
    }

    public virtual void SaveSettings(ModuleSettings settings)
    {
        if (_settingFields == null) RegisterSettings();

        foreach (var (field, attr) in _settingFields.Values)
        {
            var value = field.GetValue(this);
            settings.SetSetting(field.Name, SettingSerializer.Serialize(value));
        }
    }

    public virtual void LoadSettings(ModuleSettings settings)
    {
        if (_settingFields == null) RegisterSettings();

        foreach (var (field, attr) in _settingFields.Values)
        {
            try
            {
                var strValue = settings.GetSetting(field.Name, attr.DefaultValue);
                var value = SettingSerializer.Deserialize(strValue, field.FieldType);
                if (value != null)
                {
                    if (field.FieldType == typeof(int) && attr is IntSettingAttribute intAttr)
                    {
                        int intValue = (int)value;
                        value = Math.Clamp(intValue, intAttr.MinValue, intAttr.MaxValue);
                    }
                    else if (field.FieldType == typeof(float) && attr is FloatSettingAttribute floatAttr)
                    {
                        float floatValue = (float)value;
                        value = Math.Clamp(floatValue, floatAttr.MinValue, floatAttr.MaxValue);
                    }

                    field.SetValue(this, value);
                }
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log(LogType.Error, $"Failed to load setting {field.Name} for module {Name}. Using default. Error: {ex.Message}");
                field.SetValue(this, SettingSerializer.Deserialize(attr.DefaultValue, field.FieldType));
            }
        }
    }

    public virtual void OnImGuiSettings()
    {
        if (_settingFields == null)
            RegisterSettings();

        foreach (var (field, attr) in _settingFields.Values)
        {
            RenderFieldByType(field, attr);
        }
    }

    private void RenderFieldByType(FieldInfo field, SettingAttribute attr)
    {
        ImGui.PushID(field.Name);
        try
        {
            var currentValue = field.GetValue(this);
            var fieldType = field.FieldType;
            
            if (attr is FloatSettingAttribute floatAttr && fieldType == typeof(float))
            {
                float value = (float)currentValue;
                if (ImGui.SliderFloat(attr.DisplayName, ref value, floatAttr.MinValue, floatAttr.MaxValue, "%.2f"))
                {
                    field.SetValue(this, value);
                }
            }

            else if (attr is IntSettingAttribute intAttr && fieldType == typeof(int))
            {
                int value = (int)currentValue;
                if (ImGui.SliderInt(attr.DisplayName, ref value, intAttr.MinValue, intAttr.MaxValue))
                {
                    field.SetValue(this, value);
                }
            }

            else if (fieldType == typeof(bool))
            {
                bool value = (bool)currentValue;
                if (ImGui.Checkbox(attr.DisplayName, ref value))
                {
                    field.SetValue(this, value);
                }
            }
            else if (fieldType == typeof(int))
            {
                int value = (int)currentValue;
                if (ImGui.InputInt(attr.DisplayName, ref value))
                {
                    field.SetValue(this, value);
                }
            }
            else if (fieldType == typeof(float))
            {
                float value = (float)currentValue;
                if (ImGui.InputFloat(attr.DisplayName, ref value))
                {
                    field.SetValue(this, value);
                }
            }
            else if (fieldType == typeof(string))
            {
                string value = (string)currentValue ?? string.Empty;
                if (ImGui.InputText(attr.DisplayName, ref value, 256))
                {
                    field.SetValue(this, value);
                }
            }
            else if (fieldType == typeof(SysVec2))
            {
                SysVec2 vec = (SysVec2)currentValue;
                if (ImGui.InputFloat2(attr.DisplayName, ref vec))
                {
                    field.SetValue(this, vec);
                }
            }
            else if (fieldType == typeof(SysVec3))
            {
                SysVec3 vec = (SysVec3)currentValue;
                if (ImGui.InputFloat3(attr.DisplayName, ref vec))
                {
                    field.SetValue(this, vec);
                }
            }
            else if (fieldType == typeof(SysVec4))
            {
                SysVec4 vec = (SysVec4)currentValue;
                if (ImGui.ColorEdit4(attr.DisplayName, ref vec))
                {
                    field.SetValue(this, vec);
                }
            }
            else if (fieldType.IsEnum)
            {
                int currentIndex = (int)currentValue;
                string[] enumNames = Enum.GetNames(fieldType);
                
                if (ImGui.Combo(attr.DisplayName, ref currentIndex, enumNames, enumNames.Length))
                {
                    field.SetValue(this, Enum.Parse(fieldType, enumNames[currentIndex]));
                }
            }
            else
            {
                ImGui.Text($"{attr.DisplayName}: Unsupported type {fieldType.Name}");
            }
            
            if (!string.IsNullOrEmpty(attr.Description) && ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(attr.Description);
            }
        }
        finally
        {
            ImGui.PopID();
        }
    }

    
    private class SettingInfo
    {
        public FieldInfo FieldInfo { get; }
        public PropertyInfo PropertyInfo { get; }
        public SettingAttribute Attribute { get; }
        public string Key { get; }
        public object DefaultValue { get; }

        public SettingInfo(FieldInfo fieldInfo, SettingAttribute attribute, object defaultValue)
        {
            FieldInfo = fieldInfo;
            Attribute = attribute;
            Key = fieldInfo.Name;
            DefaultValue = defaultValue ?? fieldInfo.GetValue(null);
        }

        public SettingInfo(PropertyInfo propertyInfo, SettingAttribute attribute, object defaultValue)
        {
            PropertyInfo = propertyInfo;
            Attribute = attribute;
            Key = propertyInfo.Name;
            DefaultValue = defaultValue ?? propertyInfo.GetValue(null);
        }
    }
}

public enum ModuleCategory
{
	Movement,
	Visual,
	Exploit
}