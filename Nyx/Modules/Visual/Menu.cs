using System;
using System.Linq;
using ImGuiNET;
using Nyx.Core.Managers;
using Nyx.Core.Settings;
using UnityEngine;

namespace Nyx.Modules.Visual
{
    public class Menu : ModuleBase
    {
        private const float ModuleListWidthRatio = 0.3f;
        private const float ColorIndicatorSize = 20f;
        
        [Setting("Window Size", "Initial window size", "600,500", typeof(SysVec2))]
        private SysVec2 _windowSize = new(600, 500);
        
        private SysVec4 _enabledColor = new(0, 1, 0, 1);
        private SysVec4 _disabledColor = new(1, 0, 0, 1);
        private SysVec4 _titleColor = new(0.8f, 0.8f, 0.2f, 1);
        private SysVec4 _settingsHeaderColor = new(0.6f, 0.8f, 1, 1);

        private ModuleBase _selectedModule;
        private ModuleCategory _selectedCategory;
        private bool _showKeyBindWindow;
        private KeyCode _pendingKeyBind;

        public Menu() : base("Menu", "Shows a menu.", ModuleCategory.Visual, KeyCode.Insert)
        {
            _selectedCategory = ModuleCategory.Visual;
            RegisterSettings();
        }

        public override void OnImGuiRender()
        {
            if (!IsEnabled) return;

            RenderMainWindow();
            RenderKeyBindWindowIfNeeded();
        }

        private void RenderMainWindow()
        {
            ImGui.SetNextWindowSize(_windowSize, ImGuiCond.FirstUseEver);
            if (ImGui.Begin("Nyx Menu", ImGuiWindowFlags.NoCollapse))
            {
                RenderConfigButtons();
                _windowSize = ImGui.GetWindowSize();
                
                RenderCategoryTabs();
                RenderContentArea();
                
                ImGui.End();
            }
        }

        private void RenderContentArea()
        {
            ImGui.BeginChild("ContentArea", new SysVec2(0, -ImGui.GetFrameHeightWithSpacing()));
            {
                RenderModuleList();
                
                if (_selectedModule != null)
                {
                    ImGui.SameLine();
                    RenderModuleSettings();
                }
            }
            ImGui.EndChild();
        }

        private void RenderConfigButtons()
        {
            if (ImGui.Button("Save Settings")) SettingsManager.SaveSettings();
            ImGui.SameLine();
            if (ImGui.Button("Load Settings")) SettingsManager.LoadSettings();
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

        private void RenderModuleList()
        {
            var modules = ModuleManager.GetAllModules()
                .Where(m => m.Category == _selectedCategory)
                .OrderBy(m => m.Name)
                .ToList();

            ImGui.BeginChild("ModuleList", new SysVec2(ImGui.GetContentRegionAvail().X * ModuleListWidthRatio, 0));
            
            foreach (var module in modules)
            {
                RenderModuleListItem(module);
            }
            
            ImGui.EndChild();
        }

        private void RenderModuleListItem(ModuleBase module)
        {
            bool isSelected = _selectedModule == module;
            if (ImGui.Selectable(module.Name, isSelected, ImGuiSelectableFlags.AllowDoubleClick))
            {
                _selectedModule = isSelected ? null : module;
                if (ImGui.IsMouseDoubleClicked(0) && _selectedModule != null)
                {
                    _selectedModule.Toggle();
                }
            }
            
            string text = module.IsEnabled ? "Enabled" : "Disabled";
            ImGui.SameLine(ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X);
            ImGui.TextColored(module.IsEnabled ? _enabledColor : _disabledColor, text);
        }
        
        private void RenderModuleSettings()
        {
            ImGui.BeginChild("ModuleSettings", new SysVec2(0, 0));
            {
                RenderModuleHeader();
                RenderModuleStateControls();
                RenderModuleSpecificSettings();
            }
            ImGui.EndChild();
        }

        private void RenderModuleHeader()
        {
            ImGui.TextColored(_titleColor, _selectedModule.Name);
            ImGui.Separator();
            ImGui.TextWrapped(_selectedModule.Description);
            ImGui.Text($"Category: {_selectedModule.Category}");
        }

        private void RenderModuleStateControls()
        {
            bool isEnabled = _selectedModule.IsEnabled;
            if (ImGui.Checkbox("Enabled", ref isEnabled))
            {
                _selectedModule.Toggle();
            }
            
            RenderKeyBindSetting();
            
            if (_selectedModule.AvailableModes.Count > 0)
            {
                RenderModeSelection();
            }
        }

        private void RenderModuleSpecificSettings()
        {
            ImGui.Separator();
            ImGui.TextColored(_settingsHeaderColor, "Settings");
            _selectedModule.OnImGuiSettings();
        }

        private void RenderKeyBindSetting()
        {
            ImGui.PushID("KeyBindSetting");
            string keyName = _selectedModule.ToggleKey == KeyCode.None ? "None" : _selectedModule.ToggleKey.ToString();
            
            if (ImGui.Button($"Key Bind: {keyName}"))
            {
                _pendingKeyBind = _selectedModule.ToggleKey;
                _showKeyBindWindow = true;
            }
            
            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Click to set a new key bind");
            }
            ImGui.PopID();
        }

        private void RenderKeyBindWindowIfNeeded()
        {
            if (!_showKeyBindWindow) return;

            ImGui.SetNextWindowSize(new SysVec2(300, 120), ImGuiCond.Always);
            if (ImGui.Begin("Key Binding", ref _showKeyBindWindow, 
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking))
            {
                RenderKeyBindWindowContent();
                ImGui.End();
            }
        }

        private void RenderKeyBindWindowContent()
        {
            ImGui.TextWrapped("Press any key to set as the toggle key...");
            ImGui.Spacing();
            ImGui.Text($"Current Key: {_pendingKeyBind}");
            ImGui.Spacing();
            
            if (ImGui.Button("Clear", new SysVec2(120, 0)))
            {
                _selectedModule.SetToggleKey(KeyCode.None);
                _showKeyBindWindow = false;
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button("Cancel", new SysVec2(120, 0)))
            {
                _showKeyBindWindow = false;
            }
        }

        private void RenderModeSelection()
        {
            ImGui.PushID("ModeSelection");
            int currentMode = _selectedModule.CurrentMode;
            string[] modes = _selectedModule.AvailableModes.ToArray();
            
            ImGui.Separator();
            ImGui.Text("Mode:");
            if (ImGui.Combo("##Mode", ref currentMode, modes, modes.Length))
            {
                _selectedModule.SetMode(currentMode);
            }
            ImGui.PopID();
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

        public override void OnUpdate()
        {
            if (_showKeyBindWindow)
            {
                HandleKeyBindInput();
            }
        } 

        private void HandleKeyBindInput()
        {
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key) && key != KeyCode.None)
                {
                    _selectedModule.SetToggleKey(key);
                    _showKeyBindWindow = false;
                    break;
                }
            }
        }
    }
}
