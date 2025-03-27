using System;

namespace Nyx.SDK.Core;

/// <summary>
/// Represents data about a tracked game object including its position, visibility and dimensions.
/// </summary>
public class ObjectData
{
    // Core Properties
    public string Name { get; init; } = string.Empty;
    public WeakReference OriginalReference { get; init; } = new WeakReference(null);

    // Position Properties
    public float Distance { get; init; }
    public SysVec2 ScreenPosition { get; init; }
    public SysVec2[] BoxCorners { get; init; } = Array.Empty<SysVec2>();

    // Derived Properties
    /// <summary>
    /// Gets whether the object is currently visible on screen.
    /// </summary>
    public bool IsVisible => ScreenPosition.X > -999f;

    /// <summary>
    /// Gets the height of the object in screen space pixels.
    /// </summary>
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
