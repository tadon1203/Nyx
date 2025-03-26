using ImGuiNET;
using Nyx.Core.Managers;
using System;
using System.Linq;
using System.Numerics;

namespace Nyx.Modules.Visual;

public class Menu : ModuleBase
{
    private Vector2 _windowSize = new(600, 500);
    private ModuleBase _selectedModule;
    private ModuleCategory _selectedCategory;

    public Menu() : base("Menu", "Shows a menu.", ModuleCategory.Visual, UnityEngine.KeyCode.Insert)
    {
        _selectedCategory = ModuleCategory.Visual;
    }

    public override void OnImGuiRender()
    {
        if (!IsEnabled) return;

        ImGui.SetNextWindowSize(_windowSize, ImGuiCond.Once);
        if (ImGui.Begin("Nyx"))
        {
            if (ImGui.Button("Save config"))
            {
                ConfigManager.SaveConfig();
            }
            ImGui.SameLine();
            if (ImGui.Button("Load config"))
            {
                ConfigManager.LoadConfig();
            }

            _windowSize = ImGui.GetWindowSize();
            
            RenderCategoryTabs();
            
            RenderModulesForSelectedCategory();

            if (_selectedModule != null)
            {
                ImGui.Separator();
                RenderModuleSettings();
            }

            ImGui.End();
        }
    }

    private void RenderCategoryTabs()
    {
        var categories = ModuleManager.GetAllModules()
            .Select(m => m.Category)
            .Distinct()
            .OrderBy(c => c.ToString())
            .ToList();

        ImGui.BeginTabBar("ModuleCategoryTabs");
        
        foreach (var category in categories)
        {
            if (ImGui.BeginTabItem(category.ToString()))
            {
                _selectedCategory = category;
                ImGui.EndTabItem();
            }
        }
        
        ImGui.EndTabBar();
    }

    private void RenderModulesForSelectedCategory()
    {
        var modulesInCategory = ModuleManager.GetAllModules()
            .Where(m => m.Category == _selectedCategory)
            .ToList();

        ImGui.BeginChild("ModuleList", new Vector2(ImGui.GetContentRegionAvail().X, 300));
        
        foreach (var module in modulesInCategory)
        {
            if (ImGui.Selectable(module.Name, _selectedModule == module))
                _selectedModule = _selectedModule == module ? null : module;
        }
        
        ImGui.EndChild();
    }
    
    private void RenderModuleSettings()
    {
        ImGui.Text($"{_selectedModule.Name} Settings");

        ImGui.PushID($"{_selectedModule.Name}_toggle");
        bool isEnabled = _selectedModule.IsEnabled;
        if (ImGui.Checkbox(_selectedModule.Name, ref isEnabled))
        {
            _selectedModule.Toggle();
        }
        ImGui.PopID();

        ImGui.Text(_selectedModule.Description);
        ImGui.Text(_selectedModule.Category.ToString());
        ImGui.Separator();
        RenderHotkeySelector();
        ImGui.Separator();
        _selectedModule.OnMenu();
    }

    private void RenderHotkeySelector()
    {
        if (_selectedModule == null)
            return;

        UnityEngine.KeyCode currentKey = _selectedModule.ToggleKey;
        string currentKeyName = currentKey == UnityEngine.KeyCode.None ? "None" : currentKey.ToString();

        ImGui.Text("Toggle Key:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(200);
        if (ImGui.BeginCombo("##toggleKey", currentKeyName))
        {
            if (ImGui.Selectable("None", currentKey == UnityEngine.KeyCode.None))
                _selectedModule.SetToggleKey(UnityEngine.KeyCode.None);

            foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                UnityEngine.KeyCode key = (UnityEngine.KeyCode)Enum.Parse(typeof(UnityEngine.KeyCode), c.ToString());
                if (ImGui.Selectable(key.ToString(), _selectedModule.ToggleKey == key))
                    _selectedModule.SetToggleKey(key);
            }

            ImGui.EndCombo();
        }
    }
    
    public override void OnEnable()
    {
        if (ImGui.GetCurrentContext() == IntPtr.Zero)
            return;
        EnableCursor();
    }

    public override void OnDisable()
    {
        if (ImGui.GetCurrentContext() == IntPtr.Zero)
            return;
        DisableCursor();
    }

    private static void EnableCursor()
    {
        DearImGuiInjection.DearImGuiInjection.IsCursorVisible = true;
        ImGui.GetIO().MouseDrawCursor = true;
    }

    private static void DisableCursor()
    {
        DearImGuiInjection.DearImGuiInjection.IsCursorVisible = false;
        ImGui.GetIO().MouseDrawCursor = false;
    }
}