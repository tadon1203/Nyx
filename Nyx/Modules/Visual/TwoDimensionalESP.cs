using System.Numerics;
using ImGuiNET;
using Nyx.Core.Configuration;
using Nyx.SDK.Navigation;
using Nyx.SDK.Pickups;
using Nyx.SDK.Players;
using VRC.SDKBase;

// ReSharper disable InconsistentNaming

namespace Nyx.Modules.Visual;

public class TwoDimensionalESP() : ModuleBase("2D ESP", "Renders 2D boxes through walls.", ModuleCategory.Visual), IConfigurableModule
{
    private bool _showPlayerBoxes = true;
    private bool _showNavMeshAgentBoxes = true;
    private bool _showPickupBoxes = true;

    private float _maxDistance = 300.0f;

    private Vector4 _playerBoxColor = new(1.0f, 1.0f, 1.0f, 1.0f);
    private Vector4 _playerTextColor = new(1.0f, 1.0f, 1.0f, 1.0f);
    private Vector4 _navMeshAgentActiveColor = new(1.0f, 0.0f, 0.0f, 1.0f);
    private Vector4 _pickupAvailableColor = new(0.0f, 0.8f, 0.2f, 1.0f);

    public override void OnImGuiRender()
    {
        if (Networking.LocalPlayer == null)
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

                if (player == null || !data.IsVisible || data.Distance > _maxDistance)
                    continue;

                Draw2DBox(drawList, data.ScreenPosition, data.Distance);
            }
        }

        if (_showNavMeshAgentBoxes)
        {
            foreach (var agent in localNavMeshData)
            {
                if (!agent.IsVisible || agent.Distance > _maxDistance)
                    continue;

                Draw2DBox(drawList, agent.ScreenPosition, agent.GetHeight());
            }
        }

        if (_showPickupBoxes)
        {
            foreach (var pickup in localPickupData)
            {
                if (!pickup.IsVisible || pickup.Distance > _maxDistance)
                    continue;

                Draw2DBox(drawList, pickup.ScreenPosition, pickup.GetHeight());
            }
        }
    }

    public override void OnMenu()
    {
        ImGui.Checkbox("Show Player Boxes", ref _showPlayerBoxes);
        ImGui.Checkbox("Show NavMesh Agent Boxes", ref _showNavMeshAgentBoxes);
        ImGui.Checkbox("Show Pickup Boxes", ref _showPickupBoxes);

        ImGui.Separator();
        ImGui.Text("Color Settings");
        ImGui.ColorEdit4("Player Box Color", ref _playerBoxColor);
        ImGui.ColorEdit4("Player Text Color", ref _playerTextColor);
        ImGui.ColorEdit4("NavMeshAgent Active Color", ref _navMeshAgentActiveColor);
        ImGui.ColorEdit4("Pickup Available Color", ref _pickupAvailableColor);

        ImGui.Separator();
        ImGui.Text("Distance Settings");
        ImGui.SliderFloat("Max Distance", ref _maxDistance, 0.0f, 1000.0f);
    }

    private void Draw2DBox(ImDrawListPtr drawList, Vector2 pos, float height)
    {
        float width = height * 0.5f;
            
        Vector2 boxMin = new Vector2(pos.X - width / 2, pos.Y - height);
        Vector2 boxMax = new Vector2(pos.X + width / 2, pos.Y);
            
        uint boxColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f));
        uint outlineColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, 1.0f));

        float outlineThickness = 1.0f;

        drawList.AddRect(
            new Vector2(boxMin.X - outlineThickness, boxMin.Y - outlineThickness),
            new Vector2(boxMax.X + outlineThickness, boxMax.Y + outlineThickness),
            outlineColor, 0.0f, ImDrawFlags.RoundCornersAll, outlineThickness
        );

        drawList.AddRect(
            new Vector2(boxMin.X + outlineThickness, boxMin.Y + outlineThickness),
            new Vector2(boxMax.X - outlineThickness, boxMax.Y - outlineThickness),
            outlineColor, 0.0f, ImDrawFlags.RoundCornersAll, outlineThickness
        );
            
        drawList.AddRect(boxMin, boxMax, boxColor, 0.0f, ImDrawFlags.RoundCornersAll, 1.0f);
    }

    public void SaveModuleConfig(ModuleConfig config)
    {
        config.SetSetting("ShowPlayerBoxes", _showPlayerBoxes);
        config.SetSetting("ShowNavMeshAgentBoxes", _showNavMeshAgentBoxes);
        config.SetSetting("ShowPickupBoxes", _showPickupBoxes);

        config.SetSetting("PlayerBoxColor", _playerBoxColor);
        config.SetSetting("PlayerTextColor", _playerTextColor);
        config.SetSetting("NavMeshAgentActiveColor", _navMeshAgentActiveColor);
        config.SetSetting("PickupAvailableColor", _pickupAvailableColor);

        config.SetSetting("MaxDistance", _maxDistance);
    }

    public void LoadModuleConfig(ModuleConfig config)
    {
        _showPlayerBoxes = config.GetSetting("ShowPlayerBoxes", _showPlayerBoxes);
        _showNavMeshAgentBoxes = config.GetSetting("ShowNavMeshAgentBoxes", _showNavMeshAgentBoxes);
        _showPickupBoxes = config.GetSetting("ShowPickupBoxes", _showPickupBoxes);

        _playerBoxColor = config.GetSetting("PlayerBoxColor", _playerBoxColor);
        _playerTextColor = config.GetSetting("PlayerTextColor", _playerTextColor);
        _navMeshAgentActiveColor = config.GetSetting("NavMeshAgentActiveColor", _navMeshAgentActiveColor);
        _pickupAvailableColor = config.GetSetting("PickupAvailableColor", _pickupAvailableColor);

        _maxDistance = config.GetSetting("MaxDistance", _maxDistance);
    }
}