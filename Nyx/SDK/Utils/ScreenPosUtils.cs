using UnityEngine;

namespace Nyx.SDK.Utils;

public static class ScreenPosUtils
{
    public static SysVec2 GetScreenPositionSafe(UnityVec3 screenPosRaw)
    {
        return screenPosRaw.z > 0
            ? new SysVec2(screenPosRaw.x, Screen.height - screenPosRaw.y)
            : SysVec2.Zero;
    }
}