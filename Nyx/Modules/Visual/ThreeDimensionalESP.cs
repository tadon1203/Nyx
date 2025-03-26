using ImGuiNET;
using Nyx.Core.Settings;
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
            uint playerColor = ImGui.ColorConvertFloat4ToU32(_playerBoxColor);
            foreach (var playerEntry in PlayerManager.GetPlayerData())
            {
                if (playerEntry.Value != null && 
                    playerEntry.Value.IsVisible && 
                    playerEntry.Value.Distance <= _maxDistance && 
                    playerEntry.Value.BoxCorners != null)
                { 
                    Draw3DBox(drawList, playerEntry.Value.BoxCorners, playerColor);
                }
            }
        }


        if (_showNavMeshAgentBoxes)
        {
            uint agentColor = ImGui.ColorConvertFloat4ToU32(_navMeshAgentActiveColor);
            foreach (var agent in NavMeshManager.GetAgentData())
            {
                if (agent != null && 
                    agent.IsVisible && 
                    agent.Distance <= _maxDistance && 
                    agent.BoxCorners != null)
                {
                    Draw3DBox(drawList, agent.BoxCorners, agentColor);
                }
            }
        }
            
        if (_showPickupBoxes)
        {
            uint pickupColor = ImGui.ColorConvertFloat4ToU32(_pickupAvailableColor);
            foreach (var pickup in PickupManager.GetPickupData())
            {
                if (pickup != null && 
                    pickup.IsVisible && 
                    pickup.Distance <= _maxDistance && 
                    pickup.BoxCorners != null)
                {
                    Draw3DBox(drawList, pickup.BoxCorners, pickupColor);
                }
            }
        }
    }
    
    private void Draw3DBox(ImDrawListPtr drawList, SysVec2[] screenCorners, uint color)
    {
        int[][] edges =
        [
            [0, 1], [1, 2], [2, 3], [3, 0],
            [4, 5], [5, 6], [6, 7], [7, 4],
            [0, 4], [1, 5], [2, 6], [3, 7]
        ];

        uint fillColor = ImGui.ColorConvertFloat4ToU32(new SysVec4(
            ImGui.ColorConvertU32ToFloat4(color).X,
            ImGui.ColorConvertU32ToFloat4(color).Y,
            ImGui.ColorConvertU32ToFloat4(color).Z, 
            _fillOpacity));

        foreach (int[] edge in edges)
        {
            int a = edge[0];
            int b = edge[1];

            if (screenCorners[a].X > -999 && screenCorners[b].X > -999)
            {
                drawList.AddLine(screenCorners[a], screenCorners[b], color, _lineThickness);
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
}