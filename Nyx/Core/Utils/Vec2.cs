namespace Nyx.Core.Utils;

public class Vec2
{
    public float X;
    public float Y;

    public Vec2(float x, float y)
    {
        this.X = x;
        this.Y = y;
    }
    
    public static implicit operator Vec2(System.Numerics.Vector2 vector)
    {
        return new Vec2(vector.X, vector.Y);
    }
    
    public static implicit operator System.Numerics.Vector2(Vec2 vec2)
    {
        return new System.Numerics.Vector2(vec2.X, vec2.Y);
    }
    
    public static implicit operator Vec2(UnityEngine.Vector2 vector)
    {
        return new Vec2(vector.x, vector.y);
    }
    
    public static implicit operator UnityEngine.Vector2(Vec2 vec2)
    {
        return new UnityEngine.Vector2(vec2.X, vec2.Y);
    }
    
    public override string ToString()
    {
        return $"({X}, {Y})";
    }

}