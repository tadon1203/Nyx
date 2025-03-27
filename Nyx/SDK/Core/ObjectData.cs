using System;

namespace Nyx.SDK.Core;

public class ObjectData
{
    public string Name { get; init; }
    public float Distance { get; init; }
    public bool IsVisible { get; init; }
    public SysVec2 ScreenPosition { get; init; }
    public SysVec2[] BoxCorners { get; init; }
    public WeakReference OriginalReference { get; init; }

    public float Height => CalculateHeight();

    private float CalculateHeight()
    {
        if (BoxCorners == null || BoxCorners.Length == 0)
            return 0;

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (var corner in BoxCorners)
        {
            if (corner.Y < minY) minY = corner.Y;
            if (corner.Y > maxY) maxY = corner.Y;
        }

        return maxY - minY;
    }
}