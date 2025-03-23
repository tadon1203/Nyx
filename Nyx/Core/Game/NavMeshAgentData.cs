using UnityEngine;

namespace Nyx.Core.Game
{
    public class NavMeshAgentData
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
}