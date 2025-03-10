using System;

namespace Nyx.Core.Utils
{
	public struct Vector2
	{
		public float x, y;

		public Vector2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public static implicit operator UnityEngine.Vector2(Vector2 v)
		{
			return new UnityEngine.Vector2(v.x, v.y);
		}

		public static implicit operator System.Numerics.Vector2(Vector2 v)
		{
			return new System.Numerics.Vector2(v.x, v.y);
		}

		public static implicit operator Vector2(UnityEngine.Vector2 v)
		{
			return new Vector2(v.x, v.y);
		}

		public static implicit operator Vector2(System.Numerics.Vector2 v)
		{
			return new Vector2(v.X, v.Y);
		}

		public override string ToString()
		{
			return $"({x}, {y})";
		}

		public static Vector2 operator +(Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.x + v2.x, v1.y + v2.y);
		}

		public static Vector2 operator -(Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.x - v2.x, v1.y - v2.y);
		}

		public static Vector2 operator *(Vector2 v, float scalar)
		{
			return new Vector2(v.x * scalar, v.y * scalar);
		}

		public static Vector2 operator /(Vector2 v, float scalar)
		{
			return new Vector2(v.x / scalar, v.y / scalar);
		}
	}
}
