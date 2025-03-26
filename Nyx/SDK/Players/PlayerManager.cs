using System.Collections.Generic;
using Nyx.SDK.Constants;
using Nyx.SDK.Utils;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.SDK.Players;

public static class PlayerManager
{
    private static Dictionary<VRCPlayerApi, NyxPlayer> _playerData = new();
    private static readonly object Lock = new();
    
    public static Dictionary<VRCPlayerApi, NyxPlayer> GetPlayerData()
    {
        lock (Lock)
        {
            return new Dictionary<VRCPlayerApi, NyxPlayer>(_playerData);
        }
    }

    public static void Update()
    {
        Camera camera = Camera.main;
        if (camera == null)
            return;
        
        Vector3 cameraPosition = camera.transform.position;
        var tempPlayerData = new Dictionary<VRCPlayerApi, NyxPlayer>();

        foreach (var player in VRCPlayerApi.AllPlayers)
        {
            if (player == null || player.isLocal)
                continue;
            
            Vector3 position = player.GetPosition();
            float distance = Vector3.Distance(cameraPosition, position);
            
            float height = player.GetAvatarEyeHeightAsMeters();
            float width = height * 0.5f;
            Vector3 size = new Vector3(width, height, width);
            Vector3 center = position + new Vector3(0, height * 0.5f, 0);
            Bounds bounds = new(center, size);
            
            Vector3 screenPosRaw = camera.WorldToScreenPoint(position);
            SysVec2 screenPos = ScreenPosUtils.GetScreenPositionSafe(screenPosRaw);
            
            var data = new NyxPlayer
            {
                Name = player.displayName,
                Distance = distance,
                IsVisible = screenPosRaw.z > 0,
                ScreenPosition = screenPos,
                BoxCorners = BoundsUtils.CalculateScreenCorners(camera, bounds),
                BoneScreenPositions = new Dictionary<HumanBodyBones, SysVec2>(),
            };

            foreach (HumanBodyBones bone in BoneConstants.MainBones)
            {
                try
                {
                    Transform boneTransform = player.GetBoneTransform(bone);
                    if (boneTransform != null)
                    {
                        Vector3 boneScreenRaw = camera.WorldToScreenPoint(boneTransform.position);
                        if (boneScreenRaw.z > 0)
                        {
                            data.BoneScreenPositions[bone] = ScreenPosUtils.GetScreenPositionSafe(boneScreenRaw);
                        }
                    }
                }
                catch
                {
                    /* Ignored */
                }
            }
            tempPlayerData[player] = data;
        }
        
        lock (Lock)
        {
            _playerData = tempPlayerData;
        }
    }
}