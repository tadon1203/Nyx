using System.Numerics;
using ImGuiNET;
using Nyx.Core.Configuration;
using Nyx.Core.Utils;
using Nyx.SDK.Navigation;
using Nyx.SDK.Pickups;
using Nyx.SDK.Players;
using VRC.SDKBase;

// ReSharper disable InconsistentNaming

namespace Nyx.Modules.Visual;

public class ThreeDimensionalESP() : ModuleBase("3D ESP", "Renders 3D wireframe boxes through walls.", ModuleCategory.Visual), IConfigurableModule
{
    private bool _isEnabled = true;
    private bool _showPlayerBoxes = true;
    private bool _showNavMeshAgentBoxes = true;
    private bool _showPickupBoxes = true;

    private float _maxDistance = 300.0f;

    private Vector4 _playerBoxColor = new(1.0f, 1.0f, 1.0f, 1.0f);
    private Vector4 _navMeshAgentActiveColor = new(1.0f, 0.0f, 0.0f, 1.0f);
    private Vector4 _pickupAvailableColor = new(0.0f, 0.8f, 0.2f, 1.0f);

    public override void OnMenu()
    {
        ImGui.Checkbox("Enabled", ref _isEnabled);
        ImGui.Checkbox("Show Player Boxes", ref _showPlayerBoxes);
        ImGui.Checkbox("Show NavMesh Agent Boxes", ref _showNavMeshAgentBoxes);
        ImGui.Checkbox("Show Pickup Boxes", ref _showPickupBoxes);

        ImGui.Separator();
        ImGui.Text("Color Settings");
        ImGui.ColorEdit4("Player Box Color", ref _playerBoxColor);
        ImGui.ColorEdit4("NavMeshAgent Active Color", ref _navMeshAgentActiveColor);
        ImGui.ColorEdit4("Pickup Available Color", ref _pickupAvailableColor);

        ImGui.Separator();
        ImGui.Text("Distance Settings");
        ImGui.SliderFloat("Max Distance", ref _maxDistance, 0.0f, 1000.0f);
    }

    public override void OnImGuiRender()
    {
        if (!_isEnabled || Networking.LocalPlayer == null)
            return;

        var localPlayerData = PlayerManager.GetPlayerData();
        var localNavMeshData = NavMeshManager.GetAgentData();
        var localPickupData = PickupManager.GetPickupData();

        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

        if (_showPlayerBoxes)
        {
            foreach (var playerEntry in localPlayerData)
            {
                VRCPlayerApi player = playerEntry.Key;
                NyxPlayer data = playerEntry.Value;

                if (player == null || !data.IsVisible || data.Distance > _maxDistance || data.BoxCorners == null)
                    continue;

                Draw3DBox(drawList, data.BoxCorners, ImGui.ColorConvertFloat4ToU32(_playerBoxColor));
            }
        }

        if (_showNavMeshAgentBoxes)
        {
            foreach (var agent in localNavMeshData)
            {
                if (!agent.IsVisible || agent.Distance > _maxDistance || agent.BoxCorners == null)
                    continue;

                Draw3DBox(drawList, agent.BoxCorners, ImGui.ColorConvertFloat4ToU32(_navMeshAgentActiveColor));
            }
        }

        if (_showPickupBoxes)
        {
            foreach (var pickup in localPickupData)
            {
                if (!pickup.IsVisible || pickup.Distance > _maxDistance || pickup.BoxCorners == null)
                    continue;

                Draw3DBox(drawList, pickup.BoxCorners, ImGui.ColorConvertFloat4ToU32(_pickupAvailableColor));
            }
        }
    }

    private void Draw3DBox(ImDrawListPtr drawList, Vec2[] screenCorners, uint color)
    {
        int[][] edges =
        [
            [0, 1], [1, 2], [2, 3], [3, 0],
            [4, 5], [5, 6], [6, 7], [7, 4],
            [0, 4], [1, 5], [2, 6], [3, 7]
        ];

        uint fillColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
            ImGui.ColorConvertU32ToFloat4(color).X,
            ImGui.ColorConvertU32ToFloat4(color).Y,
            ImGui.ColorConvertU32ToFloat4(color).Z,
            0.1f));

        foreach (int[] edge in edges)
        {
            int a = edge[0];
            int b = edge[1];

            if (screenCorners[a].X > -999 && screenCorners[b].X > -999)
            {
                drawList.AddLine(screenCorners[a], screenCorners[b], color, 1.0f);
            }
        }

        int[][] faces =
        [
            [0, 1, 2, 3], 
            [4, 5, 6, 7],
            [0, 1, 5, 4],
            [1, 2, 6, 5],
            [2, 3, 7, 6],
            [3, 0, 4, 7] 
        ];

        foreach (int[] face in faces)
        {
            if (screenCorners[face[0]].X > -999 && screenCorners[face[1]].X > -999 &&
                screenCorners[face[2]].X > -999 && screenCorners[face[3]].X > -999)
            {
                drawList.AddQuadFilled(
                    screenCorners[face[0]],
                    screenCorners[face[1]],
                    screenCorners[face[2]],
                    screenCorners[face[3]],
                    fillColor
                );
            }
        }
    }

    public void SaveModuleConfig(ModuleConfig config)
    {
        config.SetSetting("Enabled", _isEnabled);
        config.SetSetting("ShowPlayerBoxes", _showPlayerBoxes);
        config.SetSetting("ShowNavMeshAgentBoxes", _showNavMeshAgentBoxes);
        config.SetSetting("ShowPickupBoxes", _showPickupBoxes);

        config.SetSetting("PlayerBoxColor", _playerBoxColor);
        config.SetSetting("NavMeshAgentActiveColor", _navMeshAgentActiveColor);
        config.SetSetting("PickupAvailableColor", _pickupAvailableColor);

        config.SetSetting("MaxDistance", _maxDistance);
    }

    public void LoadModuleConfig(ModuleConfig config)
    {
        _isEnabled = config.GetSetting("Enabled", _isEnabled);
        _showPlayerBoxes = config.GetSetting("ShowPlayerBoxes", _showPlayerBoxes);
        _showNavMeshAgentBoxes = config.GetSetting("ShowNavMeshAgentBoxes", _showNavMeshAgentBoxes);
        _showPickupBoxes = config.GetSetting("ShowPickupBoxes", _showPickupBoxes);

        _playerBoxColor = config.GetSetting("PlayerBoxColor", _playerBoxColor);
        _navMeshAgentActiveColor = config.GetSetting("NavMeshAgentActiveColor", _navMeshAgentActiveColor);
        _pickupAvailableColor = config.GetSetting("PickupAvailableColor", _pickupAvailableColor);

        _maxDistance = config.GetSetting("MaxDistance", _maxDistance);
    }
}