using ImGuiNET;
using Nyx.Core.Settings;
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

        var players = SDK.SDK.Players.ObjectsData;
        var agents = SDK.SDK.NavMeshAgents.ObjectsData;
        var pickups = SDK.SDK.Pickups.ObjectsData;

        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

        if (_showPlayerBoxes)
        {
            foreach (var player in players)
            {
                if (!player.IsVisible || player.Distance > _maxDistance)
                    continue;

                Draw2DBox(drawList, player.ScreenPosition, player.Height);
            }
        }

        if (_showNavMeshAgentBoxes)
        {
            foreach (var agent in agents)
            {
                if (!agent.IsVisible || agent.Distance > _maxDistance)
                    continue;

                Draw2DBox(drawList, agent.ScreenPosition, agent.Height);
            }
        }

        if (_showPickupBoxes)
        {
            foreach (var pickup in pickups)
            {
                if (!pickup.IsVisible || pickup.Distance > _maxDistance)
                    continue;

                Draw2DBox(drawList, pickup.ScreenPosition, pickup.Height);
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