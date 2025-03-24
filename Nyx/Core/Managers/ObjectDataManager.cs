using System.Collections.Generic;
using Nyx.Core.Game;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;

namespace Nyx.Core.Managers
{
    public static class ObjectDataManager
    {
        private static Dictionary<VRCPlayerApi, PlayerData> playerData = new();
        private static List<NavMeshAgentData> navMeshAgentData = new();
        private static List<PickupData> pickupData = new();
        private static readonly object _lock = new();
        
        private static List<NavMeshAgent> navMeshAgents = new();
        private static List<VRC_Pickup> pickups = new();
        
        private static float updateInterval = 1.0f;
        private static float timeSinceLastUpdate = 0.0f;

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

        public static Dictionary<VRCPlayerApi, PlayerData> GetPlayerData()
        {
            lock (_lock)
            {
                return new(playerData);
            }
        }

        public static List<NavMeshAgentData> GetNavMeshAgentData()
        {
            lock (_lock)
            {
                return new(navMeshAgentData);
            }
        }

        public static List<PickupData> GetPickupData()
        {
            lock (_lock)
            {
                return new(pickupData);
            }
        }

        public static void Update()
        {
            timeSinceLastUpdate += Time.deltaTime;
            if (timeSinceLastUpdate >= updateInterval)
            {
                UpdateObjectLists();
                timeSinceLastUpdate = 0.0f;
            }
            UpdateData();
        }

        private static void UpdateObjectLists()
        {
            navMeshAgents = new(Object.FindObjectsOfType<NavMeshAgent>());
            pickups = new(Object.FindObjectsOfType<VRC_Pickup>());
        }

        private static void UpdateData()
        {
            var tempPlayerData = new Dictionary<VRCPlayerApi, PlayerData>();
            var tempNavMeshData = new List<NavMeshAgentData>();
            var tempPickupData = new List<PickupData>();
            Camera camera = Camera.main;
            Vector3 cameraPosition = camera.transform.position;

            foreach (var player in VRCPlayerApi.AllPlayers)
            {
                if (player != null && !player.isLocal)
                {
                    Vector3 position = player.GetPosition();
                    float distance = Vector3.Distance(cameraPosition, position);
                
                    float height = player.GetAvatarEyeHeightAsMeters();
                    float width = height * 0.5f;
                    float depth = width;

                    Vector3 size = new Vector3(width, height, depth);
                    Vector3 center = position + new Vector3(0, height * 0.5f, 0);
                    Bounds bounds = new Bounds(center, size);

                    Vector3 screenPos = camera.WorldToScreenPoint(position);   

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

                    PlayerData data = new PlayerData
                    {
                        DisplayName = player.displayName,
                        Distance = distance,
                        IsVisible = screenPos.z > 0,
                        ScreenPosition = screenPos.z > 0 ? new Vector2(screenPos.x, Screen.height - screenPos.y) : Vector2.zero,
                        BoxCorners = screenCorners,
                        BoneScreenPositions = new(),
                    };

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
                        catch { }
                    }

                    tempPlayerData[player] = data;
                }
            }

            foreach (var agent in navMeshAgents)
            {
                if (agent != null)
                {
                    Vector3 position = agent.transform.position;
                    float distance = Vector3.Distance(cameraPosition, position);

                    float height = agent.height;
                    float radius = agent.radius;
                        
                    Vector3 size = new Vector3(radius * 2f, height, radius * 2f);
                    Vector3 center = position + new Vector3(0, height * 0.5f, 0);
                    Bounds bounds = new Bounds(center, size);

                    Vector3 screenPos = camera.WorldToScreenPoint(position);

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

                    NavMeshAgentData data = new NavMeshAgentData
                    {
                        Name = agent.gameObject.name,
                        Position = position,
                        Distance = distance,
                        IsVisible = screenPos.z > 0,
                        ScreenPosition = screenPos.z > 0 ? new Vector2(screenPos.x, Screen.height - screenPos.y) : Vector2.zero,
                        BoxCorners = screenCorners,
                        IsActive = agent.isActiveAndEnabled && agent.isOnNavMesh,
                        Destination = agent.destination,
                        Radius = radius,
                        Height = height
                    };
                    tempNavMeshData.Add(data);
                }
            }

            foreach (var pickup in pickups)
            {
                Vector3 position = pickup.transform.position;
                float distance = Vector3.Distance(cameraPosition, position);
                        
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
                
                PickupData data = new PickupData
                {
                    Name = $"{pickup.gameObject.name } | {pickup.InteractionText}",
                    Position = position,
                    Distance = distance,
                    IsVisible = screenPos.z > 0,
                    ScreenPosition = screenPos.z > 0 ? new Vector2(screenPos.x, Screen.height - screenPos.y) : Vector2.zero,
                    BoxCorners = screenCorners,
                    IsHeld = isHeld,
                    HolderName = holderName,
                    Size = size
                }; 
                tempPickupData.Add(data);     
            }

            lock (_lock)
            {
                playerData = tempPlayerData;
                navMeshAgentData = tempNavMeshData;
                pickupData = tempPickupData;
            }
        }
    }
}