namespace Nyx.Core;

public static class VectorExtensions
{
    public static SysVec2 ToSystemVector(this UnityVec2 v)
    {
        return new SysVec2(v.x, v.y);
    }

    public static UnityVec2 ToUnityVector(this SysVec2 v)
    {
        return new UnityVec2(v.X, v.Y);
    }
}