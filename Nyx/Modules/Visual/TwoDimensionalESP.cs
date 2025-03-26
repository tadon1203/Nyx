using ImGuiNET;
using Nyx.Core.Settings;
using Nyx.SDK.Navigation;
using Nyx.SDK.Pickups;
using Nyx.SDK.Players;
using VRC.SDKBase;

namespace Nyx.Modules.Visual;

public class TwoDimensionalESP : ModuleBase
{
    [FloatSetting("Max Distance", "Maximum display distance", 300.0f, 0.0f, 1000.0f)]
    private float _maxDistance = 300.0f;

    [Setting("Player Boxes", "Show player boxes", "true")]
    private bool _showPlayerBoxes = true;

    [Setting("NavMesh Agents", "Show NavMesh agent boxes", "true")]
    private bool _showNavMeshAgentBoxes = true;

    [Setting("Pickups", "Show pickup boxes", "true")]
    private bool _showPickupBoxes = true;

    public TwoDimensionalESP() : base("2D ESP", "Renders 2D boxes through walls.", ModuleCategory.Visual)
    {
        RegisterSettings();
    }

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

                Draw2DBox(drawList, data.ScreenPosition, data.GetHeight());
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
    
    private void Draw2DBox(ImDrawListPtr drawList, SysVec2 pos, float height)
    {
        float width = height * 0.5f;
                
        SysVec2 boxMin = new SysVec2(pos.X - width / 2, pos.Y - height);
        SysVec2 boxMax = new SysVec2(pos.X + width / 2, pos.Y);
                
        uint boxColor = ImGui.ColorConvertFloat4ToU32(new SysVec4(1.0f, 1.0f, 1.0f, 1.0f));
        uint outlineColor = ImGui.ColorConvertFloat4ToU32(new SysVec4(0.0f, 0.0f, 0.0f, 1.0f));

        float outlineThickness = 1.0f;

        drawList.AddRect(
            new SysVec2(boxMin.X - outlineThickness, boxMin.Y - outlineThickness),
            new SysVec2(boxMax.X + outlineThickness, boxMax.Y + outlineThickness),
            outlineColor, 0.0f, ImDrawFlags.RoundCornersAll, outlineThickness
        );

        drawList.AddRect(
            new SysVec2(boxMin.X + outlineThickness, boxMin.Y + outlineThickness),
            new SysVec2(boxMax.X - outlineThickness, boxMax.Y - outlineThickness),
            outlineColor, 0.0f, ImDrawFlags.RoundCornersAll, outlineThickness
        );
                
        drawList.AddRect(boxMin, boxMax, boxColor, 0.0f, ImDrawFlags.RoundCornersAll, 1.0f);
    }
}