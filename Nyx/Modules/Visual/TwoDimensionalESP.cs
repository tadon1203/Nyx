using ImGuiNET;
using Nyx.Core.Settings;
using Nyx.Core.Utils;
using Nyx.SDK.Core;
using VRC.SDKBase;

namespace Nyx.Modules.Visual;

public class TwoDimensionalESP : ModuleBase
{
    // Display Settings
    [FloatSetting("Max Distance", "Maximum display distance", 300.0f, 0.0f, 1000.0f)]
    private float _maxDistance = 300.0f;

    // Entity Toggles
    [Setting("Player Boxes", "Show player boxes", "true")]
    private bool _showPlayerBoxes = true;
    
    [Setting("NavMesh Agents", "Show NavMesh agent boxes", "true")] 
    private bool _showNavMeshAgentBoxes = true;
    
    [Setting("Pickups", "Show pickup boxes", "true")]
    private bool _showPickupBoxes = true;

    // Appearance Settings
    [Setting("Box Color", "Color of ESP boxes", "1.0,1.0,1.0,1.0")]
    private SysVec4 _boxColor = new(1.0f, 1.0f, 1.0f, 1.0f);
    
    [Setting("Outline Color", "Color of box outlines", "0.0,0.0,0.0,1.0")] 
    private SysVec4 _outlineColor = new(0.0f, 0.0f, 0.0f, 1.0f);

    public TwoDimensionalESP() : base("2D ESP", "Renders 2D boxes through walls.", ModuleCategory.Visual)
    {
        RegisterSettings();
    }

    public override void OnImGuiRender()
    {
        if (Networking.LocalPlayer == null)
            return;

        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
        
        if (_showPlayerBoxes)
            RenderPlayers(drawList);
            
        if (_showNavMeshAgentBoxes)
            RenderNavMeshAgents(drawList);
            
        if (_showPickupBoxes)
            RenderPickups(drawList);
    }

    private void RenderPlayers(ImDrawListPtr drawList)
    {
        foreach (var player in SDK.SDK.Players.ObjectsData)
        {
            if (ShouldRenderEntity(player))
                ESPUtils.Draw2DBox(drawList, player.ScreenPosition, player.Height, 
                    _boxColor, _outlineColor);
        }
    }

    private void RenderNavMeshAgents(ImDrawListPtr drawList)
    {
        foreach (var agent in SDK.SDK.NavMeshAgents.ObjectsData)
        {
            if (ShouldRenderEntity(agent))
                ESPUtils.Draw2DBox(drawList, agent.ScreenPosition, agent.Height,
                    _boxColor, _outlineColor);
        }
    }

    private void RenderPickups(ImDrawListPtr drawList)
    {
        foreach (var pickup in SDK.SDK.Pickups.ObjectsData)
        {
            if (ShouldRenderEntity(pickup))
                ESPUtils.Draw2DBox(drawList, pickup.ScreenPosition, pickup.Height,
                    _boxColor, _outlineColor);
        }
    }

    private bool ShouldRenderEntity(ObjectData entity)
    {
        return ESPUtils.ShouldRenderEntity(entity, _maxDistance);
    }
}
