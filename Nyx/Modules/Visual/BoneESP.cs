using System.Collections.Generic;
using ImGuiNET;
using Nyx.Core.Settings;
using Nyx.SDK.Constants;
using Nyx.SDK.Players;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.Modules.Visual
{
    public class BoneESP : ModuleBase
    {
        [FloatSetting("Max Distance", "Maximum display distance", 300.0f, 0.0f, 1000.0f)]
        private float _maxDistance = 300.0f;

        [Setting("Joint Color", "Color of joints", "0.0,0.5,1.0,1.0", typeof(SysVec4))]
        private SysVec4 _boneJointColor = new(0.0f, 0.5f, 1.0f, 1.0f);

        [Setting("Line Color", "Color of bone lines", "1.0,1.0,1.0,1.0", typeof(SysVec4))]
        private SysVec4 _boneLineColor = new(1.0f, 1.0f, 1.0f, 1.0f);

        [FloatSetting("Joint Size", "Base size of joints", 3.0f, 1.0f, 5.0f)]
        private float _baseJointSize = 3.0f;

        [FloatSetting("Line Thickness", "Thickness of lines", 1.0f, 0.5f, 2.0f)]
        private float _lineThickness = 1.0f;

        [Setting("Show Joints", "Show joints", "true")]
        private bool _showJoints = true;

        [Setting("Show Bones", "Show bones", "true")]
        private bool _showBones = true;


        public BoneESP() : base("Bone ESP", "Renders player skeleton through walls.", ModuleCategory.Visual) 
        {
            RegisterSettings();
        }

        public override void OnImGuiRender()
        {
            if (!IsEnabled || Networking.LocalPlayer == null)
                return;
            
            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
            
            foreach (var player in SDK.SDK.Players.ObjectsData)
            {
                if (!player.IsVisible || player.Distance > _maxDistance ||
                    player.BonePositions == null || player.BonePositions.Count == 0)
                    continue;

                DrawBoneEsp(drawList, player.BonePositions, player.Distance);
            }
        }

        private void DrawBoneEsp(ImDrawListPtr drawList, Dictionary<HumanBodyBones, SysVec2> bonePositions, float distance)
        {
            if (_showJoints)
            {
                uint jointColor = ImGui.ColorConvertFloat4ToU32(_boneJointColor);
                float circleRadius = _baseJointSize / (distance * 0.1f);
                circleRadius = Mathf.Clamp(circleRadius, 2.0f, 5.0f);

                foreach (var bone in bonePositions)
                {
                    drawList.AddCircleFilled(bone.Value, circleRadius, jointColor);
                }
            }

            if (_showBones)
            {
                uint lineColor = ImGui.ColorConvertFloat4ToU32(_boneLineColor);

                foreach (var connection in BoneConstants.BoneConnections)
                {
                    if (bonePositions.TryGetValue(connection.Item1, out var start) && 
                        bonePositions.TryGetValue(connection.Item2, out var end))
                    {
                        drawList.AddLine(start, end, lineColor, _lineThickness);
                    }
                }
            }
        }
    }
}
