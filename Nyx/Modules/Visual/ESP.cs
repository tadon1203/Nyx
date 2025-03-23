using System.Collections.Generic;
using ImGuiNET;
using Nyx.Core.Configuration;
using Nyx.Core.Managers;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;

namespace Nyx.Modules.Visual
{
    public class ESP : ModuleBase, IConfigurableModule
    {
        private Dictionary<VRCPlayerApi, PlayerESPData> playerData = new();
        private List<NavMeshAgentData> navMeshAgentData = new();
        private List<PickupData> pickupData = new();
        private readonly object _lock = new();

        private List<NavMeshAgent> navMeshAgents = new();
        private List<VRC_Pickup> pickups = new();
        private float updateInterval = 1.0f;
        private float timeSinceLastUpdate = 0.0f;
        
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
        
        private class PlayerESPData
        {
            public string DisplayName;
            public float Distance;
            public bool IsVisible;
            public Vector2 ScreenPosition;
            public Vector2[] BoxCorners;
            public Dictionary<HumanBodyBones, Vector2> BoneScreenPositions;
        }
        
        private class NavMeshAgentData
        {
            public string Name;
            public Vector3 Position;
            public float Distance;
            public bool IsVisible;
            public Vector2 ScreenPosition;
            public Vector2[] BoxCorners;
            public bool IsActive;
            public Vector3 Destination;
            public float Radius;
            public float Height;
        }

        private class PickupData
        {
            public string Name;
            public Vector3 Position;
            public float Distance;
            public bool IsVisible;
            public Vector2 ScreenPosition;
            public Vector2[] BoxCorners;
            public bool IsHeld;
            public string HolderName;
            public float Size;
        }

        public ESP() : base("ESP", "Highlights nearby objects and agents.", ModuleCategory.Visual) { }

        public override void OnEnable()
        {
            UpdateNavMeshAgents();
            UpdatePickups();
        }

        public override void OnDisable()
        {
            navMeshAgents.Clear();
            pickups.Clear();
        }

        public override void OnUpdate()
        {           
            var tempPlayerData = new Dictionary<VRCPlayerApi, PlayerESPData>();
            var tempNavMeshData = new List<NavMeshAgentData>();
            var tempPickupData = new List<PickupData>();
            Camera camera = Camera.main;
            Vector3 cameraPosition = camera.transform.position;

            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate >= updateInterval)
            {
                UpdateNavMeshAgents();
                UpdatePickups();
                timeSinceLastUpdate = 0.0f;
            }
            
            foreach (var player in VRCPlayerApi.AllPlayers)
            {
                if (player != null && !player.isLocal)
                {
                    Vector3 position = player.GetPosition();
                    float distance = Vector3.Distance(cameraPosition, position);
                    if (playerMaxDistance > 0 && distance > playerMaxDistance)
                        continue;

                    float height = player.GetAvatarEyeHeightAsMeters();
                    float width = height * 0.5f;
                    float depth = width;

                    Vector3 size = new Vector3(width, height, depth);
                    Vector3 center = position + new Vector3(0, height * 0.5f, 0);
                    Bounds bounds = new Bounds(center, size);
                    
                    Vector3 screenPos = camera.WorldToScreenPoint(position);
                    
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
            
            if (showNavMeshAgents)
            {
                NavMeshAgent[] allAgents = UnityEngine.Object.FindObjectsOfType<NavMeshAgent>();
                
                foreach (var agent in allAgents)
                {
                    if (agent != null)
                    {
                        Vector3 position = agent.transform.position;
                        float distance = Vector3.Distance(cameraPosition, position);
                        if (navMeshAgentMaxDistance > 0 && distance > navMeshAgentMaxDistance)
                            continue;

                        float height = agent.height;
                        float radius = agent.radius;
                        
                        Vector3 size = new Vector3(radius * 2f, height, radius * 2f);
                        Vector3 center = position + new Vector3(0, height * 0.5f, 0);
                        Bounds bounds = new Bounds(center, size);
                        
                        Vector3 screenPos = camera.WorldToScreenPoint(position);
                        
                        NavMeshAgentData data = new NavMeshAgentData
                        {
                            Name = agent.gameObject.name,
                            Position = position,
                            Distance = distance,
                            IsVisible = screenPos.z > 0,
                            ScreenPosition = screenPos.z > 0 ? new Vector2(screenPos.x, Screen.height - screenPos.y) : Vector2.zero,
                            IsActive = agent.isActiveAndEnabled && agent.isOnNavMesh,
                            Destination = agent.destination,
                            Radius = radius,
                            Height = height
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
                        
                        tempNavMeshData.Add(data);
                    }
                }
            }

            if (showPickups)
            {    
                foreach (var pickup in pickups)
                {
                    if (pickup != null)
                    {
                        Vector3 position = pickup.transform.position;
                        float distance = Vector3.Distance(cameraPosition, position);
                        if (pickupMaxDistance > 0 && distance > pickupMaxDistance)
                            continue;

                        
                        float size = pickup.GetComponent<Collider>()?.bounds.size.magnitude ?? 0.5f;
                        
                        Vector3 boundSize = new Vector3(size, size, size);
                        Vector3 center = position;
                        Bounds bounds = new Bounds(center, boundSize);
                        
                        Vector3 screenPos = camera.WorldToScreenPoint(position);
                        
                        string holderName = "";
                        bool isHeld = false;
                        
                        foreach (var playerEntry in tempPlayerData)
                        {
                            VRCPlayerApi player = playerEntry.Key;
                            if (player != null && pickup.currentPlayer == player)
                            {
                                holderName = player.displayName;
                                isHeld = true;
                                break;
                            }
                        }
                        
                        PickupData data = new PickupData
                        {
                            Name = $"{pickup.gameObject.name } | {pickup.InteractionText}",
                            Position = position,
                            Distance = distance,
                            IsVisible = screenPos.z > 0,
                            ScreenPosition = screenPos.z > 0 ? new Vector2(screenPos.x, Screen.height - screenPos.y) : Vector2.zero,
                            IsHeld = isHeld,
                            HolderName = holderName,
                            Size = size
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
                        
                        tempPickupData.Add(data);
                    }
                }
            }

            lock (_lock)
            {
                playerData = tempPlayerData;
                navMeshAgentData = tempNavMeshData;
                pickupData = tempPickupData;
            } 
        }

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
        }

        public override void OnImGuiRender()
        {
            if (!IsEnabled || Networking.LocalPlayer == null) 
                return;

            Dictionary<VRCPlayerApi, PlayerESPData> localPlayerData;
            List<NavMeshAgentData> localNavMeshData;
            List<PickupData> localPickupData;
            
            lock (_lock)
            {
                localPlayerData = new(playerData);
                localNavMeshData = new(navMeshAgentData);
                localPickupData = new(pickupData);
            }

            ImDrawListPtr drawList = ImGui.GetBackgroundDrawList();
        
            foreach (var playerEntry in localPlayerData)
            {
                VRCPlayerApi player = playerEntry.Key;
                PlayerESPData data = playerEntry.Value;
                
                if (player == null || !data.IsVisible)
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
                    if (!agent.IsVisible)
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
                    
                    if (agent.IsActive)
                    {
                        DrawAgentPath(agent, drawList);
                    }
                }
            }

            if (showPickups)
            {
                foreach (var pickup in localPickupData)
                {
                    if (!pickup.IsVisible)
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

        private void UpdateNavMeshAgents()
        {
            navMeshAgents = new(UnityEngine.Object.FindObjectsOfType<NavMeshAgent>());
        }

        private void UpdatePickups()
        {
            pickups = new(UnityEngine.Object.FindObjectsOfType<VRC_Pickup>());
        }

        // This can be the same as Draw3DBoxForAgent
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
        }
    }
}