using ImGuiNET;
using Nyx.Core.Settings;
using Nyx.Core.Utils;
using Nyx.SDK.Navigation;
using Nyx.SDK.Pickups;
using Nyx.SDK.Players;
using VRC.SDKBase;

namespace Nyx.Modules.Visual;

public class ThreeDimensionalESP : ModuleBase
{
    [FloatSetting("Max Distance", "Maximum display distance", 300.0f, 0.0f, 1000.0f)]
    private float _maxDistance = 300.0f;

    [Setting("Player Boxes", "Show player boxes", "true")]
    private bool _showPlayerBoxes = true;

    [Setting("NavMesh Agents", "Show NavMesh agents", "true")]
    private bool _showNavMeshAgentBoxes = true;

    [Setting("Pickups", "Show pickup items", "true")]
    private bool _showPickupBoxes = true;

    [Setting("Player Color", "Player color", "1.0,1.0,1.0,1.0", typeof(SysVec4))]
    private SysVec4 _playerBoxColor = new(1.0f, 1.0f, 1.0f, 1.0f);

    [Setting("Agent Color", "NavMesh agent color", "1.0,0.0,0.0,1.0", typeof(SysVec4))]
    private SysVec4 _navMeshAgentActiveColor = new(1.0f, 0.0f, 0.0f, 1.0f);

    [Setting("Pickup Color", "Pickup color", "0.0,0.8,0.2,1.0", typeof(SysVec4))]
    private SysVec4 _pickupAvailableColor = new(0.0f, 0.8f, 0.2f, 1.0f);

    [FloatSetting("Fill Opacity", "Opacity of the box fill", 0.1f, 0.0f, 1.0f)]
    private float _fillOpacity = 0.1f;

    [FloatSetting("Line Thickness", "Thickness of the lines", 1.0f, 0.5f, 2.0f)]
    private float _lineThickness = 1.0f;

    public ThreeDimensionalESP() : base("3D ESP", "Renders 3D wireframe boxes through walls.", ModuleCategory.Visual) 
    {
        RegisterSettings();
    }

    public override void OnImGuiRender()
    {
        if (Networking.LocalPlayer == null)
            return;

        var drawList = ImGui.GetBackgroundDrawList();
        
        if (_showPlayerBoxes)
        {
            foreach (var player in SDK.SDK.Players.ObjectsData)
            {
                if (ESPUtils.ShouldRenderEntity(player, _maxDistance) && player.BoxCorners != null)
                {
                    ESPUtils.Draw3DBox(drawList, player.BoxCorners, _playerBoxColor, _lineThickness, _fillOpacity);
                }
            }
        }
        
        if (_showNavMeshAgentBoxes)
        {
            foreach (var agent in SDK.SDK.NavMeshAgents.ObjectsData)
            {
                if (ESPUtils.ShouldRenderEntity(agent, _maxDistance) && agent.BoxCorners != null)
                {
                    ESPUtils.Draw3DBox(drawList, agent.BoxCorners, _navMeshAgentActiveColor, _lineThickness, _fillOpacity);
                }
            }
        }
            
        if (_showPickupBoxes)
        {
            foreach (var pickup in SDK.SDK.Pickups.ObjectsData)
            {
                if (ESPUtils.ShouldRenderEntity(pickup, _maxDistance) && pickup.BoxCorners != null)
                {
                    ESPUtils.Draw3DBox(drawList, pickup.BoxCorners, _pickupAvailableColor, _lineThickness, _fillOpacity);
                }
            }
        }
    }
}
