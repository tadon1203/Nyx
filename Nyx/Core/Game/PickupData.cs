using UnityEngine;

namespace Nyx.Core.Game
{
    public class PickupData
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
}