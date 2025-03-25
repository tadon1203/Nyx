using UnityEngine;

namespace Nyx.SDK.Utils;

public class ScreenPosUtils
{
    public static Vector2 GetScreenPositionSafe(Vector3 screenPosRaw)
    {
        return screenPosRaw.z > 0
            ? new Vector2(screenPosRaw.x, Screen.height - screenPosRaw.y)
            : Vector2.zero;
    }
}