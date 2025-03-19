using ImGuiNET;
using Nyx.Core.Configuration;
using Nyx.Core.Managers;
using Nyx.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Nyx.Modules.Visual
{
    public class Menu : ModuleBase, IConfigurableModule
    {
        private Vector2 windowSize = new(800, 650);
        private float[] columnWidths = [150.0f, 200.0f];

        private int selectedCategoryIndex = -1;
        private ModuleBase selectedModule = null;
        private int currentSelectedModule = -1;

        private List<ModuleBase> filteredModules = new();
        private ModuleCategory[] categories;

        private string searchText = string.Empty;
        private bool searchInName = true;
        private bool searchInDescription = true;

        private HashSet<string> favoriteModules = new();
        private List<string> recentModules = new();
        private bool showFavoritesOnly = false;
        private bool showRecentsOnly = false;
        private int maxRecentModules = 10;

        private enum SortOption { ByName, ByCategory, ByEnabled, ByFavorite }
        private SortOption currentSortOption = SortOption.ByName;
        private bool sortAscending = true;

        private Dictionary<string, ModuleStats> moduleStats = new();
        private GameStats gameStats = new();

        public Menu() : base("Menu", "Shows a menu.", ModuleCategory.Visual, UnityEngine.KeyCode.Insert)
        {
            categories = (ModuleCategory[])Enum.GetValues(typeof(ModuleCategory));
            InitializeGameStats();
        }

        private void InitializeGameStats()
        {
            gameStats.LaunchCount++;
            gameStats.LastLaunchTime = DateTime.Now;
        }

        public override void OnImGuiRender()
        {
            if (!IsEnabled)
                return;

            UpdateGameStats();

            RenderMainWindow();
        }

        public override void OnEnable()
        {
            if (ImGui.GetCurrentContext() == IntPtr.Zero)
                return;

            EnableCursor();
            EnsureModulesLoaded();
        }

        public override void OnDisable()
        {
            if (ImGui.GetCurrentContext() == IntPtr.Zero)
                return;

            DisableCursor();
        }

        private void RenderMainWindow()
        {
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Once);
            if (ImGui.Begin("Nyx", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar))
            {
                windowSize = ImGui.GetWindowSize();

                RenderGlobalActions();
                RenderMainLayout();

                ImGui.End();
            }

            UpdateModuleStats();
        }

        private void RenderMainLayout()
        {
            if (ImGui.BeginTable("MainLayout", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
            {
                SetupTableColumns();
                ImGui.TableHeadersRow();
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                RenderCategoriesPanel();

                ImGui.TableNextColumn();
                RenderModulesPanel();

                ImGui.TableNextColumn();
                RenderSettingsPanel();

                SaveColumnWidths();
                ImGui.EndTable();
            }
        }

        private void SetupTableColumns()
        {
            ImGui.TableSetupColumn("Categories", ImGuiTableColumnFlags.WidthFixed, columnWidths[0]);
            ImGui.TableSetupColumn("Modules", ImGuiTableColumnFlags.WidthFixed, columnWidths[1]);
            ImGui.TableSetupColumn("Settings", ImGuiTableColumnFlags.WidthStretch);
        }

        private void SaveColumnWidths()
        {
            columnWidths[0] = ImGui.GetColumnWidth(0);
            columnWidths[1] = ImGui.GetColumnWidth(1);
        }

        private void RenderCategoriesPanel()
        {
            RenderSearchBar();
            RenderFilterOptions();
            RenderSortOptions();
            RenderCategoryList();
        }

        private void RenderSearchBar()
        {
            ImGui.Text("Filter:");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(-40);
            bool searchChanged = ImGui.InputText("##Search", ref searchText, 64);

            ImGui.SameLine();
            if (ImGui.Button("×##ClearSearch"))
            {
                searchText = string.Empty;
                searchChanged = true;
            }

            RenderSearchOptions();

            if (searchChanged)
            {
                UpdateFilteredModules();
            }
        }

        private void RenderSearchOptions()
        {
            if (ImGui.BeginPopup("SearchOptions"))
            {
                ImGui.Checkbox("Search in names", ref searchInName);
                ImGui.Checkbox("Search in descriptions", ref searchInDescription);
                ImGui.EndPopup();
            }

            if (ImGui.Button("Search options##SearchOptions"))
            {
                ImGui.OpenPopup("SearchOptions");
            }
        }

        private void RenderFilterOptions()
        {
            if (ImGui.Checkbox("Favorites Only", ref showFavoritesOnly))
            {
                showRecentsOnly = false;
                UpdateFilteredModules();
            }

            ImGui.SameLine();
            if (ImGui.Checkbox("Recents Only", ref showRecentsOnly))
            {
                showFavoritesOnly = false;
                UpdateFilteredModules();
            }

            ImGui.Separator();
        }

        private void RenderSortOptions()
        {
            if (ImGui.BeginCombo("Sort By", currentSortOption.ToString()))
            {
                foreach (SortOption option in Enum.GetValues(typeof(SortOption)))
                {
                    if (ImGui.Selectable(option.ToString(), currentSortOption == option))
                    {
                        currentSortOption = option;
                        UpdateFilteredModules();
                    }
                }
                ImGui.EndCombo();
            }

            if (ImGui.Button(sortAscending ? "Descending" : "Ascending"))
            {
                sortAscending = !sortAscending;
                UpdateFilteredModules();
            }

            ImGui.Separator();
        }

        private void RenderCategoryList()
        {
            bool isSelected = selectedCategoryIndex == -1 && !showFavoritesOnly && !showRecentsOnly;
            if (ImGui.Selectable("All Modules", isSelected))
            {
                ResetCategorySelection();
                UpdateFilteredModules();
            }

            foreach (var category in categories)
            {
                isSelected = selectedCategoryIndex == (int)category;
                if (ImGui.Selectable(category.ToString(), isSelected))
                {
                    selectedCategoryIndex = (int)category;
                    selectedModule = null;
                    showFavoritesOnly = false;
                    showRecentsOnly = false;
                    UpdateFilteredModules();
                }
            }
        }

        private void ResetCategorySelection()
        {
            selectedCategoryIndex = -1;
            selectedModule = null;
            showFavoritesOnly = false;
            showRecentsOnly = false;
        }

        private void RenderModulesPanel()
        {
            EnsureModulesLoaded();

            ImGui.BeginChild("ModulesPanel");
            RenderModulesList();
            ImGui.EndChild();
        }

        private void RenderModulesList()
        {
            for (int i = 0; i < filteredModules.Count; i++)
            {
                var module = filteredModules[i];
                RenderModuleItem(module, i);
            }
        }

        private void RenderModuleItem(ModuleBase module, int index)
        {
            bool isSelected = selectedModule == module;
            ImGui.PushID(module.Name);

            float fullWidth = ImGui.GetContentRegionAvail().X;
            bool isFavorite = favoriteModules.Contains(module.Name);

            if (ImGui.Button(isFavorite ? "Unfavorite" : "Favorite"))
            {
                ToggleFavorite(module);
            }
            ImGui.SameLine();

            float checkboxWidth = 30;
            float moduleNameWidth = fullWidth - ImGui.GetCursorPosX() - checkboxWidth;

            ImGui.PushItemWidth(moduleNameWidth);
            if (ImGui.Selectable($"{module.Name}##module", isSelected, ImGuiSelectableFlags.None,
                                        new Vector2(moduleNameWidth, 0)))
            {
                selectedModule = module;
                currentSelectedModule = index;
                UpdateRecentModules(module.Name);
            }
            ImGui.PopItemWidth();

            ImGui.SameLine();
            ImGui.SetCursorPosX(fullWidth - checkboxWidth);

            bool isEnabled = module.IsEnabled;
            if (ImGui.Checkbox("##enabled", ref isEnabled))
            {
                UpdateRecentModules(module.Name);
                ToggleModuleState(module, isEnabled);
            }

            ImGui.PopID();
        }

        private void RenderSettingsPanel()
        {
            if (selectedModule == null)
            {
                RenderNoModuleSelectedView();
                return;
            }

            RenderModuleSettingsTabs();
        }

        private void RenderNoModuleSelectedView()
        {
            ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), "Select a module to view settings");
            RenderMenuHelp();

            if (ImGui.CollapsingHeader("Game Statistics"))
            {
                RenderGameStatistics();
            }
        }

        private void RenderModuleSettingsTabs()
        {
            if (ImGui.BeginTabBar("SettingsTabs"))
            {
                RenderGeneralTab();
                RenderStatisticsTab();
                RenderModuleSettingsTab();
                RenderGameStatisticsTab();
                ImGui.EndTabBar();
            }
        }

        private void RenderGeneralTab()
        {
            if (ImGui.BeginTabItem("General"))
            {
                ImGui.EndTabItem();
            }
        }

        private void RenderStatisticsTab()
        {
            if (ImGui.BeginTabItem("Statistics"))
            {
                RenderModuleStatistics();
                ImGui.EndTabItem();
            }
        }

        private void RenderModuleSettingsTab()
        {
            if (ImGui.BeginTabItem("Module Settings"))
            {
                selectedModule.OnMenu();
                ImGui.EndTabItem();
            }
        }

        private void RenderGameStatisticsTab()
        {
            if (ImGui.BeginTabItem("Game Statistics"))
            {
                RenderGameStatistics();
                ImGui.EndTabItem();
            }
        }

        private void RenderMenuHelp()
        {
            ImGui.TextWrapped("Welcome to Nyx Menu. Select a category on the left, then choose a module from the center panel to configure it.");
            ImGui.Separator();

            if (ImGui.CollapsingHeader("Favorites & Recent Modules"))
            {
                RenderFavoritesAndRecentsHelp();
            }

            if (ImGui.CollapsingHeader("Menu Configuration"))
            {
                RenderMenuConfiguration();
            }
        }

        private void RenderFavoritesAndRecentsHelp()
        {
            ImGui.TextWrapped("Click the 'Favorite' button next to a module to add it to your favorites. Use the checkboxes at the top of the category panel to filter by favorites or recently used modules.");
            ImGui.Separator();

            RenderFavoritesList();
            ImGui.Separator();
            RenderRecentsList();
        }

        private void RenderFavoritesList()
        {
            if (favoriteModules.Count > 0)
            {
                ImGui.Text("Your Favorite Modules:");
                foreach (var moduleName in favoriteModules)
                {
                    ImGui.BulletText(moduleName);
                }
            }
            else
            {
                ImGui.TextDisabled("You haven't added any favorite modules yet.");
            }
        }

        private void RenderRecentsList()
        {
            if (recentModules.Count > 0)
            {
                ImGui.Text("Recently Used Modules:");
                foreach (var moduleName in recentModules)
                {
                    ImGui.BulletText(moduleName);
                }
            }
            else
            {
                ImGui.TextDisabled("No recently used modules.");
            }
        }

        private void RenderMenuConfiguration()
        {
            if (ImGui.SliderInt("Max Recent Modules", ref maxRecentModules, 5, 20))
            {
                while (recentModules.Count > maxRecentModules)
                {
                    recentModules.RemoveAt(recentModules.Count - 1);
                }
            }
        }

        private void RenderGlobalActions()
        {
            if (ImGui.Button("Save Configuration"))
            {
                ConfigManager.SaveConfig();
            }

            ImGui.SameLine();

            if (ImGui.Button("Load Configuration"))
            {
                ConfigManager.LoadConfig();
                UpdateFilteredModules();
            }

            ImGui.SameLine();

            if (ImGui.Button("Enable All"))
            {
                EnableAllModules();
            }

            ImGui.SameLine();

            if (ImGui.Button("Disable All"))
            {
                DisableAllModules();
            }
        }

        private void EnableAllModules()
        {
            foreach (var module in ModuleManager.GetAllModules())
            {
                module.Enable();
            }
        }

        private void DisableAllModules()
        {
            foreach (var module in ModuleManager.GetAllModules())
            {
                if (module != this)
                {
                    module.Disable();
                }
            }
        }

        private void RenderModuleStatistics()
        {
            if (selectedModule == null) return;

            if (moduleStats.TryGetValue(selectedModule.Name, out ModuleStats stats))
            {
                ImGui.Text($"Times Enabled: {stats.TimesEnabled}");
                ImGui.Text($"Total Time Enabled: {FormatTime(stats.TotalTimeEnabled)}");

                if (stats.LastEnabled != DateTime.MinValue)
                {
                    ImGui.Text($"Last Enabled: {stats.LastEnabled.ToString("yyyy-MM-dd HH:mm:ss")}");
                }
                else
                {
                    ImGui.Text("Last Enabled: Never");
                }
            }
            else
            {
                ImGui.Text("No statistics available for this module.");
            }
        }

        private void RenderGameStatistics()
        {
            ImGui.Text("Game Statistics");
            ImGui.Separator();

            RenderBasicGameStats();
            ImGui.Separator();
            RenderTopModulesStats();
            ImGui.Separator();
            RenderModuleUsageDistribution();
        }

        private void RenderBasicGameStats()
        {
            ImGui.Text($"Total Playtime: {FormatTime(gameStats.TotalPlaytime)}");
            ImGui.Text($"Launch Count: {gameStats.LaunchCount}");

            if (gameStats.LastLaunchTime != DateTime.MinValue)
            {
                ImGui.Text($"Last Launch: {gameStats.LastLaunchTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                TimeSpan timeSinceLastLaunch = DateTime.Now - gameStats.LastLaunchTime;
                ImGui.Text($"Time Since Last Launch: {FormatTimeSpan(timeSinceLastLaunch)}");
            }
            else
            {
                ImGui.Text("Last Launch: Never");
            }

            if (gameStats.LaunchCount > 0)
            {
                float averageSessionTime = gameStats.TotalPlaytime / gameStats.LaunchCount;
                ImGui.Text($"Average Session Time: {FormatTime(averageSessionTime)}");
            }
        }

        private void RenderTopModulesStats()
        {
            ImGui.Text("Most Used Modules:");
            var topModules = moduleStats
                .OrderByDescending(ms => ms.Value.TotalTimeEnabled)
                .Take(5)
                .ToList();

            if (topModules.Count > 0)
            {
                for (int i = 0; i < topModules.Count; i++)
                {
                    var module = topModules[i];
                    float usagePercentage = (module.Value.TotalTimeEnabled / gameStats.TotalPlaytime) * 100f;
                    ImGui.Text($"{i + 1}. {module.Key}: {FormatTime(module.Value.TotalTimeEnabled)} ({usagePercentage:F1}%)");
                }
            }
            else
            {
                ImGui.TextDisabled("No modules used yet.");
            }
        }

        private void RenderModuleUsageDistribution()
        {
            if (moduleStats.Count > 0)
            {
                ImGui.Text("Module Usage Distribution:");
                float barWidth = ImGui.GetContentRegionAvail().X;
                float barHeight = 20.0f;

                foreach (var module in moduleStats.OrderByDescending(ms => ms.Value.TotalTimeEnabled).Take(10))
                {
                    float percentage = (module.Value.TotalTimeEnabled / gameStats.TotalPlaytime);
                    if (percentage > 0)
                    {
                        ImGui.Text($"{module.Key}");
                        ImGui.SameLine();
                        ImGui.Text($"{percentage * 100:F1}%");

                        ImGui.GetWindowDrawList().AddRectFilled(
                            new Vector2(ImGui.GetCursorScreenPos().X, ImGui.GetCursorScreenPos().Y),
                            new Vector2(ImGui.GetCursorScreenPos().X + barWidth * percentage, ImGui.GetCursorScreenPos().Y + barHeight),
                            ImGui.GetColorU32(new Vector4(0.3f, 0.5f, 0.7f, 1.0f))
                        );
                        ImGui.Dummy(new Vector2(0, barHeight + 5));
                    }
                }
            }
        }

        private void UpdateModuleStats()
        {
            foreach (var module in ModuleManager.GetAllModules())
            {
                if (!moduleStats.TryGetValue(module.Name, out ModuleStats stats))
                {
                    stats = new ModuleStats();
                    moduleStats.Add(module.Name, stats);
                }

                if (module.IsEnabled && !stats.WasEnabledLastFrame)
                {
                    stats.TimesEnabled++;
                    stats.LastEnabled = DateTime.Now;
                }

                if (module.IsEnabled)
                {
                    stats.TotalTimeEnabled += UnityEngine.Time.deltaTime;
                }

                stats.WasEnabledLastFrame = module.IsEnabled;
            }
        }

        private void ToggleFavorite(ModuleBase module)
        {
            if (favoriteModules.Contains(module.Name))
                favoriteModules.Remove(module.Name);
            else
                favoriteModules.Add(module.Name);

            UpdateFilteredModules();
        }

        private void UpdateRecentModules(string moduleName)
        {
            recentModules.Remove(moduleName);
            recentModules.Insert(0, moduleName);

            if (recentModules.Count > maxRecentModules)
            {
                recentModules.RemoveAt(recentModules.Count - 1);
            }
        }

        private void ToggleModuleState(ModuleBase module, bool isEnabled)
        {
            if (isEnabled)
                module.Enable();
            else
                module.Disable();
        }

        private void UpdateFilteredModules()
        {
            filteredModules.Clear();
            var allModules = ModuleManager.GetAllModules();

            foreach (var module in allModules)
            {
                if (ShouldIncludeModule(module))
                {
                    filteredModules.Add(module);
                }
            }

            SortModules();
        }

        private bool ShouldIncludeModule(ModuleBase module)
        {
            bool categoryMatch = selectedCategoryIndex == -1 || module.Category == (ModuleCategory)selectedCategoryIndex;
            bool favoriteMatch = !showFavoritesOnly || favoriteModules.Contains(module.Name);
            bool recentMatch = !showRecentsOnly || recentModules.Contains(module.Name);

            bool searchMatch = string.IsNullOrEmpty(searchText) ||
                             (searchInName && module.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                             (searchInDescription && module.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            return categoryMatch && searchMatch && favoriteMatch && recentMatch;
        }

        private void SortModules()
        {
            switch (currentSortOption)
            {
                case SortOption.ByName:
                    filteredModules.Sort((a, b) => sortAscending
                        ? a.Name.CompareTo(b.Name)
                        : b.Name.CompareTo(a.Name));
                    break;
                case SortOption.ByCategory:
                    filteredModules.Sort((a, b) => sortAscending
                        ? a.Category.CompareTo(b.Category)
                        : b.Category.CompareTo(a.Category));
                    break;
                case SortOption.ByEnabled:
                    filteredModules.Sort((a, b) => sortAscending
                        ? a.IsEnabled.CompareTo(b.IsEnabled)
                        : b.IsEnabled.CompareTo(a.IsEnabled));
                    break;
                case SortOption.ByFavorite:
                    filteredModules.Sort((a, b) =>
                    {
                        bool aIsFavorite = favoriteModules.Contains(a.Name);
                        bool bIsFavorite = favoriteModules.Contains(b.Name);
                        return sortAscending
                            ? aIsFavorite.CompareTo(bIsFavorite)
                            : bIsFavorite.CompareTo(aIsFavorite);
                    });
                    break;
            }
        }

        private void EnsureModulesLoaded()
        {
            if (filteredModules.Count == 0)
            {
                UpdateFilteredModules();
            }
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

        private void UpdateGameStats()
        {
            gameStats.TotalPlaytime += UnityEngine.Time.deltaTime;
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
            {
                return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m";
            }
            else if (timeSpan.TotalHours >= 1)
            {
                return $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
            else
            {
                return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
            }
        }

        private string FormatTime(float seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.TotalHours >= 1
                ? $"{time.Hours}h {time.Minutes}m {time.Seconds}s"
                : $"{time.Minutes}m {time.Seconds}s";
        }

        public void SaveModuleConfig(ModuleConfig config)
        {
            config.SetSetting("FavoriteModules", string.Join(",", favoriteModules));
            config.SetSetting("RecentModules", string.Join(",", recentModules));

            config.SetSetting("WindowWidth", windowSize.X);
            config.SetSetting("WindowHeight", windowSize.Y);
            config.SetSetting("Column0Width", columnWidths[0]);
            config.SetSetting("Column1Width", columnWidths[1]);
            config.SetSetting("SortAscending", sortAscending);
            config.SetSetting("MaxRecentModules", maxRecentModules);

            config.SetSetting("TotalPlaytime", gameStats.TotalPlaytime);
            config.SetSetting("LaunchCount", gameStats.LaunchCount);
            config.SetSetting("LastLaunchTime", gameStats.LastLaunchTime.ToString("o"));
        }

        public void LoadModuleConfig(ModuleConfig config)
        {
            LoadFavoritesAndRecents(config);
            LoadUISettings(config);
            LoadGameStats(config);

            UpdateFilteredModules();
        }

        private void LoadFavoritesAndRecents(ModuleConfig config)
        {
            string favoritesStr = config.GetSetting("FavoriteModules");
            if (!string.IsNullOrEmpty(favoritesStr))
            {
                favoriteModules = new(favoritesStr.Split(','));
            }

            string recentsStr = config.GetSetting("RecentModules");
            if (!string.IsNullOrEmpty(recentsStr))
            {
                recentModules = new(recentsStr.Split(','));
            }
        }

        private void LoadUISettings(ModuleConfig config)
        {
            float width = config.GetSetting("WindowWidth", windowSize.X);
            float height = config.GetSetting("WindowHeight", windowSize.Y);
            windowSize = new Vector2(width, height);

            columnWidths[0] = config.GetSetting("Column0Width", columnWidths[0]);
            columnWidths[1] = config.GetSetting("Column1Width", columnWidths[1]);

            sortAscending = config.GetSetting("SortAscending", sortAscending);
            maxRecentModules = config.GetSetting("MaxRecentModules", maxRecentModules);
        }

        private void LoadGameStats(ModuleConfig config)
        {
            gameStats.TotalPlaytime = config.GetSetting("TotalPlaytime", gameStats.TotalPlaytime);
            gameStats.LaunchCount = config.GetSetting("LaunchCount", gameStats.LaunchCount);

            string lastLaunchTimeStr = config.GetSetting("LastLaunchTime", string.Empty);
            if (!string.IsNullOrEmpty(lastLaunchTimeStr))
            {
                if (DateTime.TryParse(lastLaunchTimeStr, out DateTime lastLaunchTime))
                {
                    gameStats.LastLaunchTime = lastLaunchTime;
                }
            }
        }

        private class ModuleStats
        {
            public int TimesEnabled = 0;
            public float TotalTimeEnabled = 0.0f;
            public DateTime LastEnabled = DateTime.MinValue;
            public bool WasEnabledLastFrame = false;
        }

        private class GameStats
        {
            public float TotalPlaytime = 0.0f;
            public int LaunchCount = 0;
            public DateTime LastLaunchTime = DateTime.MinValue;
        }
    }
}