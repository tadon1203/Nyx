using System.Collections.Generic;
using ImGuiNET;
using Nyx.Core.Configuration;
using Nyx.Core.Game;
using Nyx.Core.Managers;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.Modules.Visual
{
    public class ESP : ModuleBase, IConfigurableModule
    {        
        private bool show2DBoxes = true;
        private bool show3DBoxes = true;
        private bool showBoneESP = true;
        private bool showNavMeshAgents = true;
        private bool showPickups = true;

        private float playerMaxDistance = 300.0f;
        private float navMeshAgentMaxDistance = 300.0f;
        private float pickupMaxDistance = 100.0f;

        private System.Numerics.Vector4 playerBoxColor = new(1.0f, 1.0f, 1.0f, 1.0f);
        private System.Numerics.Vector4 playerTextColor = new(1.0f, 1.0f, 1.0f, 1.0f);
        private System.Numerics.Vector4 boneJointColor = new(0.0f, 0.5f, 1.0f, 1.0f);
        private System.Numerics.Vector4 boneLineColor = new(1.0f, 1.0f, 1.0f, 1.0f);
        private System.Numerics.Vector4 navMeshAgentActiveColor = new(1.0f, 0.0f, 0.0f, 1.0f);
        private System.Numerics.Vector4 navMeshAgentInactiveColor = new(0.5f, 0.5f, 0.5f, 1.0f);
        private System.Numerics.Vector4 navMeshAgentPathColor = new(1.0f, 1.0f, 0.0f, 0.8f);
        private System.Numerics.Vector4 pickupHeldColor = new(0.0f, 1.0f, 0.5f, 1.0f);
        private System.Numerics.Vector4 pickupAvailableColor = new(0.0f, 0.8f, 0.2f, 1.0f);
        
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

        public ESP() : base("ESP", "Highlights nearby objects and agents.", ModuleCategory.Visual) { }

        public override void OnMenu()
        {
            ImGui.Checkbox("2D Boxes", ref show2DBoxes);
            ImGui.Checkbox("3D Wireframe", ref show3DBoxes);
            ImGui.Checkbox("Show Skeleton", ref showBoneESP);
            ImGui.Checkbox("Show NavMeshAgents", ref showNavMeshAgents);
            ImGui.Checkbox("Show Pickups", ref showPickups);

            ImGui.Separator();
            ImGui.Text("Color Settings");

            ImGui.ColorEdit4("Player Box Color", ref playerBoxColor);
            ImGui.ColorEdit4("Player Text Color", ref playerTextColor);
            ImGui.ColorEdit4("Bone Joint Color", ref boneJointColor);
            ImGui.ColorEdit4("Bone Line Color", ref boneLineColor);
            ImGui.ColorEdit4("NavMeshAgent Active Color", ref navMeshAgentActiveColor);
            ImGui.ColorEdit4("NavMeshAgent Inactive Color", ref navMeshAgentInactiveColor);
            ImGui.ColorEdit4("NavMeshAgent Path Color", ref navMeshAgentPathColor);
            ImGui.ColorEdit4("Pickup Held Color", ref pickupHeldColor);
            ImGui.ColorEdit4("Pickup Available Color", ref pickupAvailableColor);

            ImGui.Separator();
            ImGui.Text("Distance Settings");
            ImGui.SliderFloat("Player Max Distance", ref playerMaxDistance, 0.0f, 1000.0f);
            ImGui.SliderFloat("NavMeshAgent Max Distance", ref navMeshAgentMaxDistance, 0.0f, 1000.0f);
            ImGui.SliderFloat("Pickup Max Distance", ref pickupMaxDistance, 0.0f, 1000.0f);
        }

        public override void OnImGuiRender()
        {
            if (!IsEnabled || Networking.LocalPlayer == null)
                return;

            var localPlayerData = ObjectDataManager.GetPlayerData();
            var localNavMeshData = ObjectDataManager.GetNavMeshAgentData();
            var localPickupData = ObjectDataManager.GetPickupData();

            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();

            foreach (var playerEntry in localPlayerData)
            {
                VRCPlayerApi player = playerEntry.Key;
                PlayerData data = playerEntry.Value;

                if (player == null || !data.IsVisible || data.Distance > playerMaxDistance)
                    continue;

                if (show3DBoxes && data.BoxCorners != null)
                {
                    Draw3DBox(data.BoxCorners, drawList);
                }

                if (showBoneESP && data.BoneScreenPositions != null && data.BoneScreenPositions.Count > 0)
                {
                    DrawBoneESP(data.BoneScreenPositions, drawList, data.Distance);
                }

                if (show2DBoxes)
                {
                    Draw2DBox(data.DisplayName, data.ScreenPosition, data.Distance, drawList);
                }
            }

            if (showNavMeshAgents)
            {
                foreach (var agent in localNavMeshData)
                {
                    if (!agent.IsVisible || agent.Distance > navMeshAgentMaxDistance)
                        continue;

                    uint agentColor = agent.IsActive
                        ? ImGui.ColorConvertFloat4ToU32(navMeshAgentActiveColor)
                        : ImGui.ColorConvertFloat4ToU32(navMeshAgentInactiveColor);

                    if (show3DBoxes && agent.BoxCorners != null)
                    {
                        Draw3DBoxForAgent(agent.BoxCorners, drawList, agentColor);
                    }

                    if (show2DBoxes)
                    {
                        DrawAgentBox(agent, drawList, agentColor);
                    }
                }
            }

            if (showPickups)
            {
                foreach (var pickup in localPickupData)
                {
                    if (!pickup.IsVisible || pickup.Distance > pickupMaxDistance)
                        continue;

                    uint pickupColor = pickup.IsHeld
                        ? ImGui.ColorConvertFloat4ToU32(pickupHeldColor)
                        : ImGui.ColorConvertFloat4ToU32(pickupAvailableColor);

                    if (show3DBoxes && pickup.BoxCorners != null)
                    {
                        Draw3DBoxForPickup(pickup.BoxCorners, drawList, pickupColor);
                    }

                    if (show2DBoxes)
                    {
                        DrawPickupBox(pickup, drawList, pickupColor);
                    }
                }
            }
        }

        private void Draw3DBoxForPickup(Vector2[] screenCorners, ImDrawListPtr drawList, uint color)
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
                0.1f).ToSystem());

            foreach (int[] edge in edges)
            {
                int a = edge[0];
                int b = edge[1];

                if (screenCorners[a].x > -999 && screenCorners[b].x > -999)
                {
                    drawList.AddLine(screenCorners[a].ToSystem(), screenCorners[b].ToSystem(), color, 1.0f);
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

        private void DrawPickupBox(PickupData pickup, ImDrawListPtr drawList, uint color)
        {
            float boxSize = 30.0f / (pickup.Distance * 0.1f);
            boxSize = Mathf.Clamp(boxSize, 15f, 50f);
            
            Vector2 boxMin = new Vector2(pickup.ScreenPosition.x - boxSize / 2, pickup.ScreenPosition.y - boxSize / 2);
            Vector2 boxMax = new Vector2(pickup.ScreenPosition.x + boxSize / 2, pickup.ScreenPosition.y + boxSize / 2);
            
            uint outlineColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, 1.0f).ToSystem());
            float outlineThickness = 1.0f;

            drawList.AddRect(
                new Vector2(boxMin.x - outlineThickness, boxMin.y - outlineThickness).ToSystem(),
                new Vector2(boxMax.x + outlineThickness, boxMax.y + outlineThickness).ToSystem(),
                outlineColor, 0.0f, ImDrawFlags.RoundCornersAll, outlineThickness
            );

            drawList.AddRect(boxMin.ToSystem(), boxMax.ToSystem(), color, 0.0f, ImDrawFlags.RoundCornersAll, 1.0f);
            
            string statusText = pickup.IsHeld ? $"Held by {pickup.HolderName}" : "Available";
            string displayText = $"{pickup.Name} [{pickup.Distance:F1}m] - {statusText}";
            Vector2 textSize = ImGui.CalcTextSize(displayText).ToUnity();
            Vector2 textPos = new Vector2(pickup.ScreenPosition.x - textSize.x / 2, boxMin.y - textSize.y - 2);
            
            drawList.AddRectFilled(
                new Vector2(textPos.x - 2, textPos.y - 2).ToSystem(),
                new Vector2(textPos.x + textSize.x + 2, textPos.y + textSize.y + 2).ToSystem(),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.2f, 0.2f, 1.0f).ToSystem())
            );
            
            drawList.AddText(textPos.ToSystem(), color, displayText);
        }

        private void DrawAgentBox(NavMeshAgentData agent, ImDrawListPtr drawList, uint color)
        {
            float boxWidth = 40.0f / (agent.Distance * 0.1f);
            float boxHeight = 80.0f / (agent.Distance * 0.1f);
            boxWidth = Mathf.Clamp(boxWidth, 20f, 60f);
            boxHeight = Mathf.Clamp(boxHeight, 40f, 120f);
            
            Vector2 boxMin = new Vector2(agent.ScreenPosition.x - boxWidth / 2, agent.ScreenPosition.y - boxHeight);
            Vector2 boxMax = new Vector2(agent.ScreenPosition.x + boxWidth / 2, agent.ScreenPosition.y);
            
            uint outlineColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.0f, 0.0f, 0.0f, 1.0f).ToSystem());
            float outlineThickness = 1.0f;

            drawList.AddRect(
                new Vector2(boxMin.x - outlineThickness, boxMin.y - outlineThickness).ToSystem(),
                new Vector2(boxMax.x + outlineThickness, boxMax.y + outlineThickness).ToSystem(),
                outlineColor, 0.0f, ImDrawFlags.RoundCornersAll, outlineThickness
            );

            drawList.AddRect(boxMin.ToSystem(), boxMax.ToSystem(), color, 0.0f, ImDrawFlags.RoundCornersAll, 1.0f);
            
            string statusText = agent.IsActive ? "Active" : "Inactive";
            string displayText = $"{agent.Name} [{agent.Distance:F1}m] - {statusText}";
            Vector2 textSize = ImGui.CalcTextSize(displayText).ToUnity();
            Vector2 textPos = new Vector2(agent.ScreenPosition.x - textSize.x / 2, boxMin.y - textSize.y - 2);
            
            drawList.AddRectFilled(
                new Vector2(textPos.x - 2, textPos.y - 2).ToSystem(),
                new Vector2(textPos.x + textSize.x + 2, textPos.y + textSize.y + 2).ToSystem(),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.2f, 0.2f, 1.0f).ToSystem())
            );
            
            drawList.AddText(textPos.ToSystem(), color, displayText);
        }
        
        private void DrawAgentPath(NavMeshAgentData agent, ImDrawListPtr drawList)
        {
            Camera camera = Camera.main;
            if (camera == null) return;

            Vector3 destinationScreenPos = camera.WorldToScreenPoint(agent.Destination);

            if (destinationScreenPos.z <= 0) return;

            Vector2 destinationPos = new Vector2(destinationScreenPos.x, Screen.height - destinationScreenPos.y);

            uint pathColor = ImGui.ColorConvertFloat4ToU32(navMeshAgentPathColor);

            drawList.AddLine(agent.ScreenPosition.ToSystem(), destinationPos.ToSystem(), pathColor, 2.0f);

            float markerSize = 10.0f / (Vector3.Distance(camera.transform.position, agent.Destination) * 0.1f);
            markerSize = Mathf.Clamp(markerSize, 5.0f, 15.0f);

            drawList.AddCircleFilled(destinationPos.ToSystem(), markerSize, pathColor);

            float pathDistance = Vector3.Distance(agent.Position, agent.Destination);
            string distanceText = $"{pathDistance:F1}m";
            Vector2 textSize = ImGui.CalcTextSize(distanceText).ToUnity();

            Vector2 midPoint = (agent.ScreenPosition + destinationPos) * 0.5f;
            Vector2 textPos = new Vector2(midPoint.x - textSize.x * 0.5f, midPoint.y - textSize.y * 0.5f);

            drawList.AddRectFilled(
                new Vector2(textPos.x - 2, textPos.y - 2).ToSystem(),
                new Vector2(textPos.x + textSize.x + 2, textPos.y + textSize.y + 2).ToSystem(),
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.2f, 0.2f, 0.7f).ToSystem())
            );

            drawList.AddText(textPos.ToSystem(), pathColor, distanceText);
        }

        private void Draw3DBoxForAgent(Vector2[] screenCorners, ImDrawListPtr drawList, uint color)
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
                0.1f).ToSystem());

            foreach (int[] edge in edges)
            {
                int a = edge[0];
                int b = edge[1];

                if (screenCorners[a].x > -999 && screenCorners[b].x > -999)
                {
                    drawList.AddLine(screenCorners[a].ToSystem(), screenCorners[b].ToSystem(), color, 1.0f);
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

        private void Draw2DBox(string playerName, Vector2 pos, float distance, ImDrawListPtr drawList)
        {
            float boxWidth = 40.0f / (distance * 0.1f);
            float boxHeight = 80.0f / (distance * 0.1f);
            boxWidth = Mathf.Clamp(boxWidth, 20f, 60f);
            boxHeight = Mathf.Clamp(boxHeight, 40f, 120f);
            
            Vector2 boxMin = new Vector2(pos.x - boxWidth / 2, pos.y - boxHeight);
            Vector2 boxMax = new Vector2(pos.x + boxWidth / 2, pos.y);
            
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

            uint lineColor = ImGui.ColorConvertFloat4ToU32(playerBoxColor);
            uint fillColor = ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(playerBoxColor.X, playerBoxColor.Y, playerBoxColor.Z, 0.1f));

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
        
        private void DrawBoneESP(Dictionary<HumanBodyBones, Vector2> bonePositions, ImDrawListPtr drawList, float distance)
        {
            uint jointColor = ImGui.ColorConvertFloat4ToU32(boneJointColor);

            float circleRadius = 3.0f / (distance * 0.1f);
            circleRadius = Mathf.Clamp(circleRadius, 2.0f, 5.0f);

            foreach (var bone in bonePositions)
            {
                drawList.AddCircleFilled(bone.Value.ToSystem(), circleRadius, jointColor);
            }

            uint lineColor = ImGui.ColorConvertFloat4ToU32(boneLineColor);

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
            config.SetSetting("ShowNavMeshAgents", showNavMeshAgents);
            config.SetSetting("ShowPickups", showPickups);

            config.SetSetting("PlayerBoxColor", playerBoxColor);
            config.SetSetting("PlayerTextColor", playerTextColor);
            config.SetSetting("BoneJointColor", boneJointColor);
            config.SetSetting("BoneLineColor", boneLineColor);
            config.SetSetting("NavMeshAgentActiveColor", navMeshAgentActiveColor);
            config.SetSetting("NavMeshAgentInactiveColor", navMeshAgentInactiveColor);
            config.SetSetting("NavMeshAgentPathColor", navMeshAgentPathColor);
            config.SetSetting("PickupHeldColor", pickupHeldColor);
            config.SetSetting("PickupAvailableColor", pickupAvailableColor);

            config.SetSetting("PlayerMaxDistance", playerMaxDistance);
            config.SetSetting("NavMeshAgentMaxDistance", navMeshAgentMaxDistance);
            config.SetSetting("PickupMaxDistance", pickupMaxDistance);
        }

        public void LoadModuleConfig(ModuleConfig config)
        {
            show2DBoxes = config.GetSetting("Show2DBoxes", show2DBoxes);
            show3DBoxes = config.GetSetting("Show3DBoxes", show3DBoxes);
            showBoneESP = config.GetSetting("ShowBoneESP", showBoneESP);
            showNavMeshAgents = config.GetSetting("ShowNavMeshAgents", showNavMeshAgents);
            showPickups = config.GetSetting("ShowPickups", showPickups);

            playerBoxColor = config.GetSetting("PlayerBoxColor", playerBoxColor);
            playerTextColor = config.GetSetting("PlayerTextColor", playerTextColor);
            boneJointColor = config.GetSetting("BoneJointColor", boneJointColor);
            boneLineColor = config.GetSetting("BoneLineColor", boneLineColor);
            navMeshAgentActiveColor = config.GetSetting("NavMeshAgentActiveColor", navMeshAgentActiveColor);
            navMeshAgentInactiveColor = config.GetSetting("NavMeshAgentInactiveColor", navMeshAgentInactiveColor);
            navMeshAgentPathColor = config.GetSetting("NavMeshAgentPathColor", navMeshAgentPathColor);
            pickupHeldColor = config.GetSetting("PickupHeldColor", pickupHeldColor);
            pickupAvailableColor = config.GetSetting("PickupAvailableColor", pickupAvailableColor);

            playerMaxDistance = config.GetSetting("PlayerMaxDistance", playerMaxDistance);
            navMeshAgentMaxDistance = config.GetSetting("NavMeshAgentMaxDistance", navMeshAgentMaxDistance);
            pickupMaxDistance = config.GetSetting("PickupMaxDistance", pickupMaxDistance);
        }
    }
}