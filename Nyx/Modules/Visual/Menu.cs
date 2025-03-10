using ImGuiNET;
using Nyx.Core;
using Nyx.Core.Configuration;
using Nyx.Core.Managers;
using Nyx.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nyx.Modules.Visual
{
	public class Menu : ModuleBase, IConfigurableModule
	{
		private Vector2 windowSize = new(800, 650);
		private int selectedCategoryIndex = -1;
		private ModuleBase selectedModule = null;
		private ModuleCategory[] categories;
		private List<ModuleBase> filteredModules = new();
		private string searchText = string.Empty;

		private HashSet<string> favoriteModules = new HashSet<string>();
		private List<string> recentModules = new List<string>();
		private bool showFavoritesOnly = false;
		private bool showRecentsOnly = false;
		private float[] columnWidths = new float[] { 150.0f, 200.0f };
		private enum SortOption { ByName, ByCategory, ByEnabled, ByFavorite }
		private SortOption currentSortOption = SortOption.ByName;
		private bool sortAscending = true;
		private int maxRecentModules = 10;
		private int currentSelectedModule = -1;
		private bool searchInName = true;
		private bool searchInDescription = true;
		private Dictionary<string, ModuleStats> moduleStats = new Dictionary<string, ModuleStats>();
		private GameStats gameStats = new GameStats();

		public Menu() : base("Menu", "Shows a menu.", ModuleCategory.Visual, UnityEngine.KeyCode.Insert)
		{
			categories = (ModuleCategory[])Enum.GetValues(typeof(ModuleCategory));
			gameStats.LaunchCount++;
			gameStats.LastLaunchTime = DateTime.Now;
		}

		public override void OnImGuiRender()
		{
			if (!IsEnabled)
				return;

			gameStats.TotalPlaytime += UnityEngine.Time.deltaTime;

			ImGui.SetNextWindowSize(windowSize, ImGuiCond.Once);
			if (ImGui.Begin("Nyx", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar))
			{
				windowSize = ImGui.GetWindowSize();

				RenderGlobalActions();

				if (ImGui.BeginTable("MainLayout", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
				{
					ImGui.TableSetupColumn("Categories", ImGuiTableColumnFlags.WidthFixed, columnWidths[0]);
					ImGui.TableSetupColumn("Modules", ImGuiTableColumnFlags.WidthFixed, columnWidths[1]);
					ImGui.TableSetupColumn("Settings", ImGuiTableColumnFlags.WidthStretch);
					ImGui.TableHeadersRow();

					ImGui.TableNextRow();

					ImGui.TableNextColumn();
					RenderCategoriesPanel();

					ImGui.TableNextColumn();
					RenderModulesPanel();

					ImGui.TableNextColumn();
					RenderSettingsPanel();

					columnWidths[0] = ImGui.GetColumnWidth(0);
					columnWidths[1] = ImGui.GetColumnWidth(1);

					ImGui.EndTable();
				}

				ImGui.End();
			}

			UpdateModuleStats();
		}

		private void RenderCategoriesPanel()
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

			if (searchChanged)
			{
				UpdateFilteredModules();
			}

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

			if (ImGui.Button(sortAscending ? "Decending" : "Ascending"))
			{
				sortAscending = !sortAscending;
				UpdateFilteredModules();
			}

			ImGui.Separator();

			bool isSelected = selectedCategoryIndex == -1 && !showFavoritesOnly && !showRecentsOnly;
			if (ImGui.Selectable("All Modules", isSelected))
			{
				selectedCategoryIndex = -1;
				selectedModule = null;
				showFavoritesOnly = false;
				showRecentsOnly = false;
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

		private void UpdateFilteredModules()
		{
			filteredModules.Clear();

			var allModules = ModuleManager.GetAllModules();

			foreach (var module in allModules)
			{
				bool categoryMatch = selectedCategoryIndex == -1 || module.Category == (ModuleCategory)selectedCategoryIndex;
				bool favoriteMatch = !showFavoritesOnly || favoriteModules.Contains(module.Name);
				bool recentMatch = !showRecentsOnly || recentModules.Contains(module.Name);

				bool searchMatch = string.IsNullOrEmpty(searchText) ||
								 (searchInName && module.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
								 (searchInDescription && module.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase));

				if (categoryMatch && searchMatch && favoriteMatch && recentMatch)
				{
					filteredModules.Add(module);
				}
			}

			SortModules();
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

		private void RenderModulesPanel()
		{
			if (filteredModules.Count == 0)
			{
				UpdateFilteredModules();
			}

			ImGui.BeginChild("ModulesPanel");

			for (int i = 0; i < filteredModules.Count; i++)
			{
				var module = filteredModules[i];
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
					currentSelectedModule = i;
					UpdateRecentModules(module.Name);
				}
				ImGui.PopItemWidth();

				ImGui.SameLine();
				ImGui.SetCursorPosX(fullWidth - checkboxWidth);

				bool isEnabled = module.IsEnabled;
				if (ImGui.Checkbox("##enabled", ref isEnabled))
				{
					UpdateRecentModules(module.Name);
					if (isEnabled)
						module.Enable();
					else
						module.Disable();
				}

				ImGui.PopID();
			}

			ImGui.EndChild();
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

		private void RenderSettingsPanel()
		{
			if (selectedModule == null)
			{
				ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1.0f), "Select a module to view settings");
				RenderMenuHelp();

				// ゲーム統計のタブをメニューヘルプの後に表示
				if (ImGui.CollapsingHeader("Game Statistics"))
				{
					RenderGameStatistics();
				}

				return;
			}

			if (ImGui.BeginTabBar("SettingsTabs"))
			{
				if (ImGui.BeginTabItem("General"))
				{
					// 既存のコード
					ImGui.EndTabItem();
				}

				if (ImGui.BeginTabItem("Statistics"))
				{
					RenderModuleStatistics();
					ImGui.EndTabItem();
				}

				if (ImGui.BeginTabItem("Module Settings"))
				{
					selectedModule.OnMenu();
					ImGui.EndTabItem();
				}

				if (ImGui.BeginTabItem("Game Statistics"))
				{
					RenderGameStatistics();
					ImGui.EndTabItem();
				}

				ImGui.EndTabBar();
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

				// add some graphs?
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

			// 平均プレイ時間（セッションごと）を計算して表示
			if (gameStats.LaunchCount > 0)
			{
				float averageSessionTime = gameStats.TotalPlaytime / gameStats.LaunchCount;
				ImGui.Text($"Average Session Time: {FormatTime(averageSessionTime)}");
			}

			ImGui.Separator();

			// 最も利用されているモジュールのトップ5を表示
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

			ImGui.Separator();

			// 使用状況を可視化するためのグラフ（簡易版）
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

						// 使用率のバー表示
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

		private void RenderMenuHelp()
		{
			ImGui.TextWrapped("Welcome to Nyx Menu. Select a category on the left, then choose a module from the center panel to configure it.");

			ImGui.Separator();

			if (ImGui.CollapsingHeader("Favorites & Recent Modules"))
			{
				ImGui.TextWrapped("Click the 'Favorite' button next to a module to add it to your favorites. Use the checkboxes at the top of the category panel to filter by favorites or recently used modules.");

				ImGui.Separator();

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

				ImGui.Separator();

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

			if (ImGui.CollapsingHeader("Menu Configuration"))
			{
				if (ImGui.SliderInt("Max Recent Modules", ref maxRecentModules, 5, 20))
				{
					while (recentModules.Count > maxRecentModules)
					{
						recentModules.RemoveAt(recentModules.Count - 1);
					}
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
				foreach (var module in ModuleManager.GetAllModules())
				{
					module.Enable();
				}
			}

			ImGui.SameLine();

			if (ImGui.Button("Disable All"))
			{
				foreach (var module in ModuleManager.GetAllModules())
				{
					if (module != this)
					{
						module.Disable();
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

		public override void OnEnable()
		{
			if (ImGui.GetCurrentContext() == IntPtr.Zero)
				return;

			DearImGuiInjection.DearImGuiInjection.IsCursorVisible = true;
			ImGui.GetIO().MouseDrawCursor = true;

			if (filteredModules.Count == 0)
			{
				UpdateFilteredModules();
			}
		}

		public override void OnDisable()
		{
			if (ImGui.GetCurrentContext() == IntPtr.Zero)
				return;

			DearImGuiInjection.DearImGuiInjection.IsCursorVisible = false;
			ImGui.GetIO().MouseDrawCursor = false;
		}

		public void SaveModuleConfig(ModuleConfig config)
		{
			config.SetSetting("FavoriteModules", string.Join(",", favoriteModules));
			config.SetSetting("RecentModules", string.Join(",", recentModules));

			config.SetSetting("WindowWidth", windowSize.x);
			config.SetSetting("WindowHeight", windowSize.y);
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

			float width = config.GetSetting("WindowWidth", windowSize.x);
			float height = config.GetSetting("WindowHeight", windowSize.y);
			windowSize = new Vector2(width, height);
			columnWidths[0] = config.GetSetting("Column0Width", columnWidths[0]);
			columnWidths[1] = config.GetSetting("Column1Width", columnWidths[1]);
			sortAscending = config.GetSetting("SortAscending", sortAscending);
			maxRecentModules = config.GetSetting("MaxRecentModules", maxRecentModules);

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

			UpdateFilteredModules();
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
