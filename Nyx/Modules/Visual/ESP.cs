using System.Collections.Generic;
using ImGuiNET;
using Nyx.Core.Configuration;
using Nyx.Core.Managers;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.Modules.Visual
{
    public class ESP : ModuleBase, IConfigurableModule
    {
        private Dictionary<VRCPlayerApi, PlayerESPData> playerData = new();
        private readonly object _lock = new();
        
        private bool show2DBoxes = true;
        private bool show3DBoxes = true;
        private bool showBoneESP = true;
        
        private static readonly (HumanBodyBones, HumanBodyBones)[] boneConnections = 
        {
            (HumanBodyBones.Hips, HumanBodyBones.Spine),
            (HumanBodyBones.Spine, HumanBodyBones.Chest),
            (HumanBodyBones.Chest, HumanBodyBones.UpperChest),
            (HumanBodyBones.UpperChest, HumanBodyBones.Neck),
            (HumanBodyBones.Neck, HumanBodyBones.Head),
            
            (HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm),
            (HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm),
            (HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand),
            
            (HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm),
            (HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm),
            (HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand),
            
            (HumanBodyBones.Hips, HumanBodyBones.LeftUpperLeg),
            (HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg),
            (HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot),
            
            (HumanBodyBones.Hips, HumanBodyBones.RightUpperLeg),
            (HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg),
            (HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot),
            
            (HumanBodyBones.UpperChest, HumanBodyBones.LeftShoulder),
            (HumanBodyBones.UpperChest, HumanBodyBones.RightShoulder)
        };
        
        private static readonly HumanBodyBones[] mainBones = 
        {
            HumanBodyBones.Hips,
            HumanBodyBones.Spine,
            HumanBodyBones.Chest,
            HumanBodyBones.UpperChest,
            HumanBodyBones.Neck,
            HumanBodyBones.Head,
            HumanBodyBones.LeftShoulder,
            HumanBodyBones.LeftUpperArm,
            HumanBodyBones.LeftLowerArm,
            HumanBodyBones.LeftHand,
            HumanBodyBones.RightShoulder,
            HumanBodyBones.RightUpperArm,
            HumanBodyBones.RightLowerArm,
            HumanBodyBones.RightHand,
            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.LeftFoot,
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.RightLowerLeg,
            HumanBodyBones.RightFoot
        };
        
        private class PlayerESPData
        {
            public string DisplayName;
            public float Distance;
            public bool IsVisible;
            public Vector2 ScreenPosition;
            public Vector2[] BoxCorners;
            public Dictionary<HumanBodyBones, Vector2> BoneScreenPositions;
        }

        public ESP() : base("ESP", "Highlights nearby objects.", ModuleCategory.Visual) { }

        public override void OnUpdate()
        {
            if (!IsEnabled || Networking.LocalPlayer == null) 
                return;
                
            var tempPlayerData = new Dictionary<VRCPlayerApi, PlayerESPData>();
            Camera camera = Camera.main;
            Vector3 cameraPosition = camera.transform.position;
                
            foreach (var player in VRCPlayerApi.AllPlayers)
            {
                if (player != null && !player.isLocal)
                {
                    Vector3 position = player.GetPosition();
                    float height = player.GetAvatarEyeHeightAsMeters();
                    float width = height * 0.5f;
                    float depth = width;

                    Vector3 size = new Vector3(width, height, depth);
                    Vector3 center = position + new Vector3(0, height * 0.5f, 0);
                    Bounds bounds = new Bounds(center, size);
                    
                    Vector3 screenPos = camera.WorldToScreenPoint(position);
                    
                    float distance = Vector3.Distance(cameraPosition, position);
                    
                    PlayerESPData data = new PlayerESPData
                    {
                        DisplayName = player.displayName,
                        Distance = distance,
                        IsVisible = screenPos.z > 0,
                        ScreenPosition = screenPos.z > 0 ? new Vector2(screenPos.x, Screen.height - screenPos.y) : Vector2.zero
                    };
                    
                    if (show2DBoxes || show3DBoxes)
                    {
                        Vector2[] screenCorners = new Vector2[8];
                        Vector3[] corners = new Vector3[]
                        {
                            new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
                            new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                            new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                            new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), 
                            new Vector3(bounds.max.x, bounds.max.y, bounds.max.z),
                            new Vector3(bounds.min.x, bounds.max.y, bounds.max.z)
                        };
                        
                        for (int i = 0; i < 8; i++)
                        {
                            Vector3 cornerScreenPos = camera.WorldToScreenPoint(corners[i]);
                            screenCorners[i] = cornerScreenPos.z > 0 
                                ? new Vector2(cornerScreenPos.x, Screen.height - cornerScreenPos.y) 
                                : new Vector2(-1000, -1000);
                        }
                        
                        data.BoxCorners = screenCorners;
                    }
                    
                    if (showBoneESP)
                    {
                        data.BoneScreenPositions = new Dictionary<HumanBodyBones, Vector2>();
                        
                        foreach (HumanBodyBones bone in mainBones)
                        {
                            try
                            {
                                Transform boneTransform = player.GetBoneTransform(bone);
                                if (boneTransform != null)
                                {
                                    Vector3 boneWorldPos = boneTransform.position;
                                    Vector3 boneScreenPos = camera.WorldToScreenPoint(boneWorldPos);
                                    
                                    if (boneScreenPos.z > 0)
                                    {
                                        data.BoneScreenPositions[bone] = new Vector2(boneScreenPos.x, Screen.height - boneScreenPos.y);
                                    }
                                }
                            }
                            catch
                            {
                                
                            }
                        }
                    }
                    
                    tempPlayerData[player] = data;
                }
            }
                
            lock (_lock)
            {
                playerData = tempPlayerData;
            } 
        }

        public override void OnMenu()
        {
            ImGui.Checkbox("2D Boxes", ref show2DBoxes);
            ImGui.Checkbox("3D Wireframe", ref show3DBoxes);
            ImGui.Checkbox("Show Skeleton", ref showBoneESP);
        }

        public override void OnImGuiRender()
        {
            if (!IsEnabled || Networking.LocalPlayer == null) 
                return;

            Dictionary<VRCPlayerApi, PlayerESPData> localPlayerData;
            
            lock (_lock)
            {
                localPlayerData = new Dictionary<VRCPlayerApi, PlayerESPData>(playerData);
            }

            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
        
            foreach (var playerEntry in localPlayerData)
            {
                VRCPlayerApi player = playerEntry.Key;
                PlayerESPData data = playerEntry.Value;
                
                if (player == null || !data.IsVisible)
                    continue;
                
                if (show2DBoxes)
                {
                    Draw2DBox(data.DisplayName, data.ScreenPosition, data.Distance, drawList);
                }
                
                if (show3DBoxes && data.BoxCorners != null)
                {
                    Draw3DBox(data.BoxCorners, drawList);
                }
                
                if (showBoneESP && data.BoneScreenPositions != null && data.BoneScreenPositions.Count > 0)
                {
                    DrawBoneESP(data.BoneScreenPositions, drawList);
                }
            }
        }

        private void Draw2DBox(string playerName, Vector2 pos, float distance, ImDrawListPtr drawList)
        {
            float boxWidth = 40.0f / (distance * 0.1f);
            float boxHeight = 80.0f / (distance * 0.1f);
            boxWidth = Mathf.Clamp(boxWidth, 20f, 60f);
            boxHeight = Mathf.Clamp(boxHeight, 40f, 120f);
            
            Vector2 boxMin = new Vector2(pos.x - boxWidth / 2, pos.y - boxHeight / 2);
            Vector2 boxMax = new Vector2(pos.x + boxWidth / 2, pos.y + boxHeight / 2);
            
            uint boxColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f).ToSystem());
            uint outlineColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, 1.0f).ToSystem());
        
            float outlineThickness = 1.0f;

            drawList.AddRect(
                new Vector2(boxMin.x - outlineThickness, boxMin.y - outlineThickness).ToSystem(),
                new Vector2(boxMax.x + outlineThickness, boxMax.y + outlineThickness).ToSystem(),
                outlineColor, 0.0f, ImDrawFlags.RoundCornersAll, outlineThickness
            );

            drawList.AddRect(
                new Vector2(boxMin.x + outlineThickness, boxMin.y + outlineThickness).ToSystem(),
                new Vector2(boxMax.x - outlineThickness, boxMax.y - outlineThickness).ToSystem(),
                outlineColor, 0.0f, ImDrawFlags.RoundCornersAll, outlineThickness
            );
            
            drawList.AddRect(boxMin.ToSystem(), boxMax.ToSystem(), boxColor, 0.0f, ImDrawFlags.RoundCornersAll, 1.0f);
            
            string displayText = $"{playerName} [{distance:F1}m]";
            Vector2 textSize = ImGui.CalcTextSize(displayText).ToUnity();
            Vector2 textPos = new Vector2(pos.x - textSize.x / 2, boxMin.y - textSize.y - 2);
            
            drawList.AddRectFilled(
                new Vector2(textPos.x - 2, textPos.y - 2).ToSystem(),
                new Vector2(textPos.x + textSize.x + 2, textPos.y + textSize.y + 2).ToSystem(),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.2f, 0.2f, 1.0f).ToSystem())
            );
            
            drawList.AddText(textPos.ToSystem(), boxColor, displayText);
        }

        private void Draw3DBox(Vector2[] screenCorners, ImDrawListPtr drawList)
        {
            int[][] edges =
            [
                [0, 1], [1, 2], [2, 3], [3, 0],
                [4, 5], [5, 6], [6, 7], [7, 4],
                [0, 4], [1, 5], [2, 6], [3, 7]
            ];

            uint lineColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f).ToSystem());
            uint fillColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 0.2f).ToSystem());

            foreach (int[] edge in edges)
            {
                int a = edge[0];
                int b = edge[1];

                if (screenCorners[a].x > -999 && screenCorners[b].x > -999)
                {
                    drawList.AddLine(screenCorners[a].ToSystem(), screenCorners[b].ToSystem(), lineColor, 1.0f);
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
                if (screenCorners[face[0]].x > -999 && screenCorners[face[1]].x > -999 &&
                    screenCorners[face[2]].x > -999 && screenCorners[face[3]].x > -999)
                {
                    drawList.AddQuadFilled(
                        screenCorners[face[0]].ToSystem(),
                        screenCorners[face[1]].ToSystem(),
                        screenCorners[face[2]].ToSystem(),
                        screenCorners[face[3]].ToSystem(),
                        fillColor
                    );
                }
            }
        }
        
        private void DrawBoneESP(Dictionary<HumanBodyBones, Vector2> bonePositions, ImDrawListPtr drawList)
        {
            uint jointColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.5f, 1.0f, 1.0f).ToSystem());
            
            foreach (var bone in bonePositions)
            {
                drawList.AddCircleFilled(bone.Value.ToSystem(), 3.0f, jointColor);
            }
            
            uint lineColor = ImGui.ColorConvertFloat4ToU32(new Vector4(1.0f, 1.0f, 1.0f, 1.0f).ToSystem());
            
            foreach (var connection in boneConnections)
            {
                if (bonePositions.TryGetValue(connection.Item1, out Vector2 start) && 
                    bonePositions.TryGetValue(connection.Item2, out Vector2 end))
                {
                    drawList.AddLine(start.ToSystem(), end.ToSystem(), lineColor, 1.0f);
                }
            }
        }

        public void SaveModuleConfig(ModuleConfig config)
		{
			config.SetSetting("Show2DBoxes", show2DBoxes);
			config.SetSetting("Show3DBoxes", show3DBoxes);
			config.SetSetting("ShowBoneESP", showBoneESP);
		}

		public void LoadModuleConfig(ModuleConfig config)
		{
			show2DBoxes = config.GetSetting("Show2DBoxes", show2DBoxes);
			show3DBoxes = config.GetSetting("Show3DBoxes", show3DBoxes);
			showBoneESP = config.GetSetting("ShowBoneESP", showBoneESP);
		}
    }
}