using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nyx.Core.Utils
{
	public struct Vector4
	{
		public float x, y, z, w;

		public Vector4(float x, float y, float z, float w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		public static implicit operator UnityEngine.Vector4(Vector4 v)
		{
			return new UnityEngine.Vector4(v.x, v.y, v.z, v.w);
		}

		public static implicit operator System.Numerics.Vector4(Vector4 v)
		{
			return new System.Numerics.Vector4(v.x, v.y, v.z, v.w);
		}

		public static implicit operator Vector4(UnityEngine.Vector4 v)
		{
			return new Vector4(v.x, v.y, v.z, v.w);
		}

		public static implicit operator Vector4(System.Numerics.Vector4 v)
		{
			return new Vector4(v.X, v.Y, v.Z, v.W);
		}

		public override string ToString()
		{
			return $"({x}, {y}, {z}, {w})";
		}

		public static Vector4 operator +(Vector4 v1, Vector4 v2)
		{
			return new Vector4(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z, v1.w + v2.w);
		}

		public static Vector4 operator -(Vector4 v1, Vector4 v2)
		{
			return new Vector4(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z, v1.w - v2.w);
		}

		public static Vector4 operator *(Vector4 v, float scalar)
		{
			return new Vector4(v.x * scalar, v.y * scalar, v.z * scalar, v.w * scalar);
		}

		public static Vector4 operator /(Vector4 v, float scalar)
		{
			return new Vector4(v.x / scalar, v.y / scalar, v.z / scalar, v.w / scalar);
		}

		public static Vector4 FromColorCode(string colorCode)
		{
			if (string.IsNullOrEmpty(colorCode))
			{
				return new Vector4(0f, 0f, 0f, 1f);
			}

			colorCode = colorCode.Trim();
			if (!colorCode.StartsWith("#"))
			{
				return new Vector4(0f, 0f, 0f, 1f);
			}
			colorCode = colorCode.Substring(1); // remove '#'

			if (colorCode.Length == 6) // #RRGGBB
			{
				float r = HexToFloat(colorCode.Substring(0, 2));
				float g = HexToFloat(colorCode.Substring(2, 2));
				float b = HexToFloat(colorCode.Substring(4, 2));
				return new Vector4(r, g, b, 1f);
			}
			else if (colorCode.Length == 8) // #RRGGBBAA
			{
				float r = HexToFloat(colorCode.Substring(0, 2));
				float g = HexToFloat(colorCode.Substring(2, 2));
				float b = HexToFloat(colorCode.Substring(4, 2));
				float a = HexToFloat(colorCode.Substring(6, 2));
				return new Vector4(r, g, b, a);
			}
			else
			{
				return new Vector4(0f, 0f, 0f, 1f);
			}
		}

		private static float HexToFloat(string hex)
		{
			int value = int.Parse(hex, NumberStyles.HexNumber);
			return value / 255f;
		}
	}
}
