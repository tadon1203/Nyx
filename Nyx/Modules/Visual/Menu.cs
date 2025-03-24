using ImGuiNET;
using Nyx.Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nyx.Modules.Visual
{
    public class Menu : ModuleBase
    {
        private Vector2 windowSize = new(600, 500);
        private ModuleBase selectedModule = null;
        private Dictionary<ModuleCategory, bool> categoryExpanded;

        public Menu() : base("Menu", "Shows a menu.", ModuleCategory.Visual, UnityEngine.KeyCode.Insert)
        {
            categoryExpanded = Enum.GetValues(typeof(ModuleCategory))
                .Cast<ModuleCategory>()
                .ToDictionary(c => c, c => true);
        }

        public override void OnImGuiRender()
        {
            if (!IsEnabled) return;

            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Once);
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

                windowSize = ImGui.GetWindowSize();
                RenderModules();

                if (selectedModule != null)
                {
                    ImGui.Separator();
                    RenderModuleSettings();
                }

                ImGui.End();
            }
        }

        private void RenderModules()
        {
            var modulesByCategory = ModuleManager.GetAllModules().GroupBy(m => m.Category);
            foreach (var group in modulesByCategory)
            {
                ImGui.Text($"{group.Key}");
                ImGui.Separator();

                foreach (var module in group)
                {
                    if (ImGui.Selectable(module.Name, selectedModule == module))
                        selectedModule = selectedModule == module ? null : module;
                }
                ImGui.NewLine();
            }
        }

        private void RenderModuleSettings()
        {
            ImGui.Text($"{selectedModule.Name} Settings");

            ImGui.PushID($"{selectedModule.Name}_toggle");
            bool isEnabled = selectedModule.IsEnabled;
            if (ImGui.Checkbox(selectedModule.Name, ref isEnabled))
            {
                selectedModule.Toggle();
            }
            ImGui.PopID();

            ImGui.Text(selectedModule.Description);
            ImGui.Text(selectedModule.Category.ToString());
            ImGui.Separator();
            RenderHotkeySelector();
            ImGui.Separator();
            selectedModule.OnMenu();
        }

        private void RenderHotkeySelector()
        {
            if (selectedModule == null)
                return;

            UnityEngine.KeyCode currentKey = selectedModule.ToggleKey;
            string currentKeyName = currentKey == UnityEngine.KeyCode.None ? "None" : currentKey.ToString();

            ImGui.Text("Toggle Key:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            if (ImGui.BeginCombo("##toggleKey", currentKeyName))
            {
                if (ImGui.Selectable("None", currentKey == UnityEngine.KeyCode.None))
                    selectedModule.SetToggleKey(UnityEngine.KeyCode.None);

                foreach (char c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
                {
                    UnityEngine.KeyCode key = (UnityEngine.KeyCode)Enum.Parse(typeof(UnityEngine.KeyCode), c.ToString());
                    if (ImGui.Selectable(key.ToString(), selectedModule.ToggleKey == key))
                        selectedModule.SetToggleKey(key);
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

        private void EnableCursor()
        {
            DearImGuiInjection.DearImGuiInjection.IsCursorVisible = true;
            ImGui.GetIO().MouseDrawCursor = true;
        }

        private void DisableCursor()
        {
            DearImGuiInjection.DearImGuiInjection.IsCursorVisible = false;
            ImGui.GetIO().MouseDrawCursor = false;
        }
    }
}