namespace Nyx.Core.Utils;

public class Vec3
{
    public float X;
    public float Y;
    public float Z;

    public Vec3(float x, float y, float z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }
    
    public static implicit operator Vec3(System.Numerics.Vector3 vector)
    {
        return new Vec3(vector.X, vector.Y, vector.Z);
    }
    
    public static implicit operator System.Numerics.Vector3(Vec3 vec3)
    {
        return new System.Numerics.Vector3(vec3.X, vec3.Y, vec3.Z);
    }
    
    public static implicit operator Vec3(UnityEngine.Vector3 vector)
    {
        return new Vec3(vector.x, vector.y, vector.z);
    }
    
    public static implicit operator UnityEngine.Vector3(Vec3 vec3)
    {
        return new UnityEngine.Vector3(vec3.X, vec3.Y, vec3.Z);
    }
    
    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }
    
    public static Vec3 Zero => new Vec3(0f, 0f, 0f);
    
    public static Vec3 One => new Vec3(1f, 1f, 1f);
    
    public static Vec3 Up => new Vec3(0f, 1f, 0f);
    
    public static Vec3 Down => new Vec3(0f, -1f, 0f);
    
    public static Vec3 Right => new Vec3(1f, 0f, 0f);
    
    public static Vec3 Left => new Vec3(-1f, 0f, 0f);
    
    public static Vec3 Forward => new Vec3(0f, 0f, 1f);
    
    public static Vec3 Back => new Vec3(0f, 0f, -1f);
}