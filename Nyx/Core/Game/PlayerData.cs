using System.Collections.Generic;
using UnityEngine;

namespace Nyx.Core.Game
{
    public class PlayerData
    {
        public string DisplayName;
        public float Distance;
        public bool IsVisible;
        public Vector2 ScreenPosition;
        public Vector2[] BoxCorners;
        public Dictionary<HumanBodyBones, Vector2> BoneScreenPositions;
    }
}