using UnityEngine;

namespace Nyx.SDK.Constants;

public static class BoneConstants
{
    public static readonly (HumanBodyBones, HumanBodyBones)[] BoneConnections =
    [
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
    ];

    public static readonly HumanBodyBones[] MainBones =
    [
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
    ];
}