using UnityEngine;
using System.Numerics;

namespace Nyx.Core.Managers
{
    public static class VectorExtensions
    {
        public static System.Numerics.Vector2 ToSystem(this UnityEngine.Vector2 unityVector)
        {
            return new System.Numerics.Vector2(unityVector.x, unityVector.y);
        }

        public static System.Numerics.Vector3 ToSystem(this UnityEngine.Vector3 unityVector)
        {
            return new System.Numerics.Vector3(unityVector.x, unityVector.y, unityVector.z);
        }

        public static System.Numerics.Vector4 ToSystem(this UnityEngine.Vector4 unityVector)
        {
            return new System.Numerics.Vector4(unityVector.x, unityVector.y, unityVector.z, unityVector.w);
        }

        public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 numericsVector)
        {
            return new UnityEngine.Vector2(numericsVector.X, numericsVector.Y);
        }

        public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 numericsVector)
        {
            return new UnityEngine.Vector3(numericsVector.X, numericsVector.Y, numericsVector.Z);
        }

        public static UnityEngine.Vector4 ToUnity(this System.Numerics.Vector4 numericsVector)
        {
            return new UnityEngine.Vector4(numericsVector.X, numericsVector.Y, numericsVector.Z, numericsVector.W);
        }
    }
}