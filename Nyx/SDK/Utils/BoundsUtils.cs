using Nyx.Core.Utils;
using UnityEngine;

namespace Nyx.SDK.Utils;

public static class BoundsUtils
{
    public static Vec2[] CalculateScreenCorners(Camera camera, Bounds bounds)
    {
        Vec3[] corners =
        [
            new Vec3(bounds.min.x, bounds.min.y, bounds.min.z),
            new Vec3(bounds.max.x, bounds.min.y, bounds.min.z),
            new Vec3(bounds.max.x, bounds.min.y, bounds.max.z),
            new Vec3(bounds.min.x, bounds.min.y, bounds.max.z),
            new Vec3(bounds.min.x, bounds.max.y, bounds.min.z),
            new Vec3(bounds.max.x, bounds.max.y, bounds.min.z),
            new Vec3(bounds.max.x, bounds.max.y, bounds.max.z),
            new Vec3(bounds.min.x, bounds.max.y, bounds.max.z)
        ];

        Vec2[] screenCorners = new Vec2[8];
        for (int i = 0; i < 8; i++)
        {
            Vec3 screenPosRaw = camera.WorldToScreenPoint(corners[i]);
            screenCorners[i] = screenPosRaw.Z > 0
                ? new Vec2(screenPosRaw.X, Screen.height - screenPosRaw.Y)
                : new Vec2(-1000, -1000);
        }
        return screenCorners;
    }
}