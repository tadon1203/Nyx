using UnityEngine;

namespace Nyx.SDK.Utils;

public static class ScreenUtils
{
    public static SysVec2 WorldToScreenPoint(Camera camera, UnityVec3 worldPosition)
    {
        var screenPos = camera.WorldToScreenPoint(worldPosition);
        return screenPos.z > 0 
            ? new SysVec2(screenPos.x, Screen.height - screenPos.y)
            : new SysVec2(-999, -999);
    }

    public static bool IsVisible(Camera camera, UnityVec3 worldPosition) 
        => camera.WorldToScreenPoint(worldPosition).z > 0;
}