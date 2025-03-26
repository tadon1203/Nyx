namespace Nyx.SDK.Navigation;

public class NyxNavMeshAgent
{
    public string Name;
    public float Distance;
    public bool IsVisible;
    public SysVec2 ScreenPosition;
    public SysVec2[] BoxCorners;
    
    public float GetHeight()
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