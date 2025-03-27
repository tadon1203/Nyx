using System;

namespace Nyx.SDK.Core;
public class ObjectData
{
    public string Name { get; init; } = string.Empty;
    public WeakReference OriginalReference { get; init; } = new(null);
    
    public float Distance { get; init; }
    public SysVec2 ScreenPosition { get; init; }
    public SysVec2[] BoxCorners { get; init; } = [];
    
    public bool IsVisible => ScreenPosition.X > -999f;
    
    public float Height => CalculateHeight();

    private float CalculateHeight()
    {
        if (BoxCorners is not { Length: > 0 })
            return 0f;

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (var corner in BoxCorners)
        {
            minY = Math.Min(minY, corner.Y);
            maxY = Math.Max(maxY, corner.Y);
        }

        return maxY - minY;
    }
}
