using System;
using System.Collections.Generic;
using System.Linq;
using Nyx.SDK.Constants;
using Nyx.SDK.Core;
using Nyx.SDK.Utils;
using UnityEngine;
using VRC.SDKBase;

namespace Nyx.SDK.Players;

public class PlayerManager : BaseManager<VRCPlayerApi, PlayerData>
{
    public override void FindObjects()
        => TrackedObjects = VRCPlayerApi.AllPlayers.ToArray().ToList();

    protected override PlayerData CreateObjectData(VRCPlayerApi player)
    {
        var position = player.GetPosition();
        var camera = Camera.main;
        
        return new PlayerData
        {
            Name = player.displayName,
            Distance = Vector3.Distance(camera.transform.position, position),
            IsVisible = ScreenUtils.IsVisible(camera, position),
            ScreenPosition = ScreenUtils.WorldToScreenPoint(camera, position),
            BoxCorners = CalculatePlayerBounds(camera, player),
            BonePositions = GetBonePositions(camera, player),
            OriginalReference = new WeakReference(player)
        };
    }

    private SysVec2[] CalculatePlayerBounds(Camera camera, VRCPlayerApi player)
    {
        float height = player.GetAvatarEyeHeightAsMeters();
        float width = height * 0.5f;
        var bounds = new Bounds(
            player.GetPosition() + new Vector3(0, height * 0.5f, 0),
            new Vector3(width, height, width));
            
        return CalculateBoxCorners(camera, bounds);
    }

    private Dictionary<HumanBodyBones, SysVec2> GetBonePositions(Camera camera, VRCPlayerApi player)
    {
        var bonePositions = new Dictionary<HumanBodyBones, SysVec2>();
            
        foreach (var bone in BoneConstants.MainBones)
        {
            try
            {
                var boneTransform = player.GetBoneTransform(bone);
                if (boneTransform != null)
                {
                    bonePositions[bone] = 
                        ScreenUtils.WorldToScreenPoint(camera, boneTransform.position);
                }
            }
            catch
            {
                // ignored
            }
        }
            
        return bonePositions;
    }
}