using System.Collections.Generic;
using ImGuiNET;
using Nyx.Core.Configuration;
using Nyx.Core.Utils;
using Nyx.SDK.Constants;
using Nyx.SDK.Players;
using UnityEngine;
using VRC.SDKBase;

// ReSharper disable InconsistentNaming

namespace Nyx.Modules.Visual;

public class BoneESP : ModuleBase, IConfigurableModule
{
    private bool _isEnabled = true;
    private bool _showPlayerBones = true;

    private float _maxDistance = 300.0f;

    private System.Numerics.Vector4 _boneJointColor = new(0.0f, 0.5f, 1.0f, 1.0f);
    private System.Numerics.Vector4 _boneLineColor = new(1.0f, 1.0f, 1.0f, 1.0f);

    public BoneESP() : base("Bone ESP", "Renders player skeleton through walls.", ModuleCategory.Visual) { }

    public override void OnMenu()
    {
        ImGui.Checkbox("Enabled", ref _isEnabled);
        ImGui.Checkbox("Show Player Bones", ref _showPlayerBones);

        ImGui.Separator();
        ImGui.Text("Color Settings");
        ImGui.ColorEdit4("Bone Joint Color", ref _boneJointColor);
        ImGui.ColorEdit4("Bone Line Color", ref _boneLineColor);

        ImGui.Separator();
        ImGui.Text("Distance Settings");
        ImGui.SliderFloat("Max Distance", ref _maxDistance, 0.0f, 1000.0f);
    }

    public override void OnImGuiRender()
    {
        if (!_isEnabled || Networking.LocalPlayer == null)
            return;

        var localPlayerData = PlayerManager.GetPlayerData();
        ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

        if (_showPlayerBones)
        {
            foreach (var playerEntry in localPlayerData)
            {
                VRCPlayerApi player = playerEntry.Key;
                NyxPlayer data = playerEntry.Value;

                if (player == null || !data.IsVisible || data.Distance > _maxDistance ||
                    data.BoneScreenPositions == null || data.BoneScreenPositions.Count == 0)
                    continue;

                DrawBoneEsp(drawList, data.BoneScreenPositions, data.Distance);
            }
        }
    }

    private void DrawBoneEsp(ImDrawListPtr drawList, Dictionary<HumanBodyBones, Vec2> bonePositions, float distance)
    {
        uint jointColor = ImGui.ColorConvertFloat4ToU32(_boneJointColor);

        float circleRadius = 3.0f / (distance * 0.1f);
        circleRadius = Mathf.Clamp(circleRadius, 2.0f, 5.0f);
        

        foreach (var bone in bonePositions)
        {
            drawList.AddCircleFilled(bone.Value, circleRadius, jointColor);
        }

        uint lineColor = ImGui.ColorConvertFloat4ToU32(_boneLineColor);

        foreach (var connection in BoneConstants.BoneConnections)
        {
            if (bonePositions.TryGetValue(connection.Item1, out var start) && 
                bonePositions.TryGetValue(connection.Item2, out var end))
            {
                drawList.AddLine(start, end, lineColor, 1.0f);
            }
        }
    }

    public void SaveModuleConfig(ModuleConfig config)
    {
        config.SetSetting("Enabled", _isEnabled);
        config.SetSetting("ShowPlayerBones", _showPlayerBones);

        config.SetSetting("BoneJointColor", _boneJointColor);
        config.SetSetting("BoneLineColor", _boneLineColor);

        config.SetSetting("MaxDistance", _maxDistance);
    }

    public void LoadModuleConfig(ModuleConfig config)
    {
        _isEnabled = config.GetSetting("Enabled", _isEnabled);
        _showPlayerBones = config.GetSetting("ShowPlayerBones", _showPlayerBones);

        _boneJointColor = config.GetSetting("BoneJointColor", _boneJointColor);
        _boneLineColor = config.GetSetting("BoneLineColor", _boneLineColor);

        _maxDistance = config.GetSetting("MaxDistance", _maxDistance);
    }
}