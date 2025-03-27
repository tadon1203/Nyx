using System.Collections.Generic;
using Nyx.SDK.Core;
using UnityEngine;

namespace Nyx.SDK.Players;

public class PlayerData : ObjectData
{
    public Dictionary<HumanBodyBones, SysVec2> BonePositions { get; init; } = new();
}