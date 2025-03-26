using UnityEngine;

namespace Nyx.SDK.Utils;

public static class BoundsUtils
{
    public static SysVec2[] CalculateScreenCorners(Camera camera, Bounds bounds)
    {
        Vector3[] corners =
        [
            new Vector3(bounds.min.x, bounds.min.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.max.y, bounds.max.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.max.z)
        ];

        SysVec2[] screenCorners = new SysVec2[8];
        for (int i = 0; i < 8; i++)
        {
            Vector3 screenPosRaw = camera.WorldToScreenPoint(corners[i]);
            screenCorners[i] = screenPosRaw.z > 0
                ? new SysVec2(screenPosRaw.x, Screen.height - screenPosRaw.y)
                : new SysVec2(-1000, -1000);
        }
        return screenCorners;
    }
}